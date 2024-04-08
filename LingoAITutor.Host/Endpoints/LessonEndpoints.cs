using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Services;
using LingoAITutor.Host.Services.LessonProgress;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Endpoints
{
    public static class LessonEndpoints
    {
        public static void AddEndpoints(WebApplication application)
        {
            application.MapGet("api/lessons", GetLessons).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Get user lessons list",
            });
            application.MapGet("api/lessons/{lessonId}", GetLesson).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Get user lesson",
            });
            application.MapPost("api/lessons", CreateLesson).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Create lesson",
            });
            application.MapDelete("api/lessons/{lessonId}", DeleteLesson).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Delete lesson",
            });
            application.MapPost("api/lessons/{lessonId}/submitMessage", SubmitMessage).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Submit message",
            });
            application.MapPost("api/lessons/dirtyCheck", DirtyCheck).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Check grammar in the sentences dirty",
            });
            application.MapPost("api/lessons/hint", GuessMissingWord).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Get word hint for text",
            });
            application.MapPut("api/lessons/{lessonId}/progressLesson", ProgressLesson).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Submit message",
            });
        }

        private static async Task<IResult> GetLessons(LingoDbContext dbContext, UserIdHepler userIdHelper)
        {
            var lessons = await dbContext.Lessons.Include(l=> l.Scenario).Where(l => l.UserId == userIdHelper.GetUserId()).OrderByDescending(l => l.Created).ToArrayAsync();
            return Results.Ok(lessons.Select(ch => new LessonDto() 
            {
                LessonId = ch.LessonId,
                Title = ch.Title,
                Created = ch.Created,                
                LowQualityCount = ch.LowQualityCount,
                MessagesCount = ch.MessagesCount,
                RevisedCount = ch.RevisedCount
            }).ToArray());
        }

        private static async Task<IResult> GetLesson(LingoDbContext dbContext, UserIdHepler userIdHelper, Guid lessonId)
        {
            var lesson = await dbContext.Lessons.Include(l => l.Scenario).FirstOrDefaultAsync(l => l.LessonId == lessonId);
            if (lesson == null)
            {
                return Results.NotFound();
            }
            return Results.Ok(new LessonDto()
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Created = lesson.Created,
                Preface = string.IsNullOrWhiteSpace(lesson.Scenario.Preface) ? lesson.Scenario.Description: lesson.Scenario.Preface,
                LowQualityCount = lesson.LowQualityCount,
                MessagesCount = lesson.MessagesCount,
                RevisedCount = lesson.RevisedCount,                
            });
        }

        private static async Task<IResult> CreateLesson(LingoDbContext dbContext, UserIdHepler userIdHelper, LessonProgressor lessonProgressor, 
                                                        [FromBody] ScenarioOptionsDto options)
        {
            var scenario = dbContext.ScenarioTemplates.FirstOrDefault(l => l.Id == options.ScenarioId);
            if (scenario is null)
            {
                return Results.NotFound();
            }
            var lessonId = Guid.NewGuid();
            var newLesson = new Lesson()
            {
                LessonId = lessonId,
                UserId = userIdHelper.GetUserId(),
                ScenarioId = options.ScenarioId,                
                Created = DateTime.UtcNow,                                                 
                Title = scenario.Title,
                MessagesCount = 0,
                NextQuestionRandom = scenario.NextQuestionRandom
            };
            dbContext.Add(newLesson);
            await dbContext.SaveChangesAsync();            
            await lessonProgressor.ProgressLesson(newLesson, scenario);
            return Results.Ok(lessonId.ToString());
        }

        private static async Task<IResult> DirtyCheck(LingoDbContext dbContext, UserIdHepler userIdHelper, GrammarChecker grammarChecker,
                                                         [FromBody] RequestByText text)
        {
            var correctedText = await grammarChecker.FixGrammar(text.Text);
            var corrections = CorrectionsComposer.ComposeCorrections(text.Text!, correctedText);
            return Results.Ok(new DirtyCheckResultDto() { Corrections = corrections, FixedText = correctedText });
        }

        private static async Task<IResult> GuessMissingWord(LingoDbContext dbContext, UserIdHepler userIdHelper, MissingWordGuesser missingWordGuesser,
                                                         [FromBody] RequestByTextAndPos hintRequest)
        {
            var guessedWords = await missingWordGuesser.GuessMissingWord(hintRequest.Text!, hintRequest.Position);            
            return Results.Ok(guessedWords);
        }

        private static async Task<IResult> SubmitMessage(LingoDbContext dbContext, UserIdHepler userIdHelper, GrammarChecker grammarChecker, 
                                                         Guid lessonId, [FromBody] MessageDto message)
        {
            var lesson = await dbContext.Lessons.FirstOrDefaultAsync(m => m.LessonId == lessonId);
            if (lesson == null)
                return Results.NotFound();
            if (lesson.UserId != userIdHelper.GetUserId())
                return Results.Unauthorized();
            var fixedContent = await grammarChecker.FixGrammar(message.Content);
            lesson.LastMessageNumber++;
            lesson.MessagesCount++;
            var newMessage = new Message()
            {
                MessageId = Guid.NewGuid(),
                Content = message.Content,
                CorrectedContent = fixedContent,
                Corrections = CorrectionsComposer.ComposeCorrections(message.Content!, fixedContent),
                MessageType = Entities.Enums.MessageType.UserMessage,
                Number = lesson.LastMessageNumber,
                SectionNumber = lesson.SectionNumber                
            };
            lesson.Messages.Add(newMessage);
            dbContext.Add(newMessage);
            await dbContext.SaveChangesAsync();
            return Results.Ok(new MessageDto()
            {
                Content = newMessage.Content,
                CorrectedContent = fixedContent,
                Corrections = newMessage.Corrections,
                MessageId = newMessage.MessageId,
                MessageType = newMessage.MessageType
            });
        }

        private static async Task<IResult> ProgressLesson(LingoDbContext dbContext, UserIdHepler userIdHelper, LessonProgressor lessonProgressor, Guid lessonId)
        {
            var lesson = await dbContext.Lessons.Include(l=> l.Scenario).FirstOrDefaultAsync(m => m.LessonId == lessonId);
            if (lesson == null)
                return Results.NotFound();
            if (lesson.UserId != userIdHelper.GetUserId())
                return Results.Unauthorized();
            var gptMessage = await lessonProgressor.ProgressLesson(lesson, lesson.Scenario);
            if (gptMessage ==null)
                return Results.NotFound();
            return Results.Ok(new MessageDto()
            {
                Content = gptMessage.Content,
                MessageId = gptMessage.MessageId,
                MessageType = Entities.Enums.MessageType.GPTMessage
            });
        }

        private static async Task<IResult> DeleteLesson(LingoDbContext dbContext, UserIdHepler userIdHelper, LessonProgressor lessonProgressor, Guid lessonId)
        {
            var lesson = await dbContext.Lessons.Include(ch => ch.Messages).FirstOrDefaultAsync(m => m.LessonId == lessonId);
            if (lesson == null)
                return Results.NotFound();
            if (lesson.UserId != userIdHelper.GetUserId())
                return Results.Unauthorized();
            dbContext.RemoveRange(lesson.Messages);
            dbContext.Remove(lesson);
            await dbContext.SaveChangesAsync();
            return Results.Ok();
        }

    }

}

