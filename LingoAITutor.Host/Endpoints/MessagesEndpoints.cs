using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Endpoints
{
    public static class MessagesEndpoints
    {
        public static void AddEndpoints(WebApplication application)
        {
            application.MapGet("api/lessons/{lessonId}/messages", GetMessages).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Get all lesson messages",
            });
        }

        private static async Task<IResult> GetMessages(LingoDbContext dbContext, UserIdHepler userIdHelper, Guid lessonId)
        {
            var lesson = await dbContext.Lessons.Where(ch => ch.LessonId == lessonId).Include(ch => ch.Messages).FirstOrDefaultAsync();
            if (lesson is null || lesson.UserId != userIdHelper.GetUserId())
                return Results.NotFound("Lesson not found");
            return Results.Ok(lesson.Messages.OrderBy(m=>m.Number).Select(m => new MessageDto()
            {
                MessageId = m.MessageId,
                Content = m.Content,
                CorrectedContent = m.CorrectedContent,
                Corrections = m.Corrections,
                MessageType = m.MessageType
            }).ToArray());
        }

    }
}
