using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Entities.Enums;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace LingoAITutor.Host.Services.LessonProgress.QuestionsLessonProgress
{
    public class QuestionsLessonProgressor : ILessonProgressor
    {

        private readonly LingoDbContext _dbContext;
        private readonly OpenAIAPI _openAPI;

        public QuestionsLessonProgressor(LingoDbContext dbContext, OpenAIAPI openAPI)
        {
            _dbContext = dbContext;
            _openAPI = openAPI;
        }

        public async Task<Message?> ProgressLesson(Lesson lesson)
        {
            var notStarted = lesson.MessagesCount == 0;
            if (notStarted)
            {
                return await StartLesson(lesson);
            }

            var messages = await _dbContext.Messages.Where(m => m.LessonId == lesson.LessonId && m.SectionNumber == lesson.SectionNumber)
                           .ToArrayAsync();
            messages = messages.OrderBy(m => m.Number).ToArray();

            ValidateMessageListToProgress(messages);

            var chatMessages = ConvertToAIAnalisedForm(messages);

            var gptResponseText = await GetGPTResponse(chatMessages);

            return await CreateAndAddNewMessage(lesson, gptResponseText);
        }

        private void ValidateMessageListToProgress(Message[] messages)
        {
            if (messages.Last().MessageType != MessageType.UserMessage)
                throw new Exception("Progress possible olny after user message");
        }

        private async Task<Message?> CreateAndAddNewMessage(Lesson lesson, string messageText)
        {
            lesson.LastMessageNumber++;
            lesson.MessagesCount++;
            var gptMessage = new Message()
            {
                MessageId = Guid.NewGuid(),
                Content = messageText,
                MessageType = MessageType.GPTMessage,
                Number = lesson.LastMessageNumber,
                SectionNumber = lesson.SectionNumber
            };
            lesson.Messages.Add(gptMessage);
            _dbContext.Add(gptMessage);
            await _dbContext.SaveChangesAsync();
            return gptMessage;
        }

        private async Task<Message?> StartLesson(Lesson lesson)
        {
            var question = NextQuestionSelector.SelectNextQuestion(lesson);
            if (question.number == -1)
                throw new Exception("Empty question list.");

            var startMessage = new Message()
            {
                MessageId = Guid.NewGuid(),
                MessageType = MessageType.GPTMessage,
                Content = question.text,
                SectionInitMessage = true,
                Number = 0,
                SectionNumber = 0
            };
            lesson.ProgressInfo = lesson.ProgressInfo + (string.IsNullOrWhiteSpace(lesson.ProgressInfo) ? "" : ",") + question.number;
            lesson.MessagesCount = 1;
            lesson.SectionNumber = 0;
            lesson.Messages.Add(startMessage);
            _dbContext.Messages.Add(startMessage);
            await _dbContext.SaveChangesAsync();
            return startMessage;
        }

        public async Task<Message?> ToNextSection(Lesson lesson, ScenarioTemplate scenario)
        {
            if (lesson.MessagesCount == 0)
                return null;

            var question = NextQuestionSelector.SelectNextQuestion(lesson);
            if (question.number == -1)
                return null;

            var lastMessage = await _dbContext.Messages.Where(m => m.LessonId == lesson.LessonId && m.SectionNumber == lesson.SectionNumber)
                                    .OrderByDescending(m => m.Number).Take(1).ToArrayAsync();

            if (lastMessage[0].SectionInitMessage)
            {
                var pi = RemoveNumber(lesson.ProgressInfo, FindNumber(scenario.Content, lastMessage[0].Content));
                lesson.ProgressInfo = pi + (string.IsNullOrWhiteSpace(pi) ? "" : ",") + question.number;
                lastMessage[0].Content = question.text;
                await _dbContext.SaveChangesAsync();
                return null;
            }
            else
            {
                lesson.LastMessageNumber++;
                lesson.SectionNumber++;

                var message = new Message()
                {
                    MessageId = Guid.NewGuid(),
                    MessageType = MessageType.GPTMessage,
                    Content = question.text,
                    Number = lesson.LastMessageNumber,
                    SectionNumber = lesson.SectionNumber,
                    SectionInitMessage = true
                };
                lesson.ProgressInfo = lesson.ProgressInfo + (string.IsNullOrWhiteSpace(lesson.ProgressInfo) ? "" : ",") + question.number;
                lesson.Messages.Add(message);
                _dbContext.Add(message);
                await _dbContext.SaveChangesAsync();
                return message;
            }
        }

        private int FindNumber(string? scenarioContent, string? questionContent)
        {
            return scenarioContent!.Split(Environment.NewLine).ToList().IndexOf(questionContent!);
        }

        private string RemoveNumber(string? progressInfo, int number)
        {
            return string.Join(',', NextQuestionSelector.ParsePassedQuestionNumbers(progressInfo).Where(n => n != number));
        }

        private async Task<string> GetGPTResponse(IList<ChatMessage> messages)
        {
            var result = await _openAPI.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0,
                MaxTokens = 1000,
                Messages = messages
            });
            return result.Choices[0].Message.Content;
        }

        private List<ChatMessage> ConvertToAIAnalisedForm(Message[] messages)
        {
            // первое сообщение - это вопрос, его не учитываем.
            // Второе - это ответ, его комбинируем с первым, получаем вопрос пользователя;
            // последующие - это обсуждение;
            // если они есть, просто переписываем их в историю.
            var result = new List<ChatMessage>();

            if (messages.Length < 2)
                return result;

            var questionAndAnswer = CombineAnswerAndResponce(messages[0].Content, messages[1].Content, ".Net developer");
            result.Add(new ChatMessage(ChatMessageRole.User, questionAndAnswer));

            for (var i = 2; i < messages.Length; i++)
            {
                var message = messages[i];
                result.Add(new ChatMessage(message.MessageType == MessageType.GPTMessage ? ChatMessageRole.Assistant : ChatMessageRole.User, message.Content));
            }
            return result;
        }

        private string CombineAnswerAndResponce(string? question, string? answer, string subject)
        {
            return "I am preparing for an interview for position " + subject + ". I want to prepare an answer to the following question:\n" +
            "\"" + question + "\"\n" +
            "Below is my answer to this question.Analyze my answer.Find inaccuracies in it and point them out. Estimate the completeness of the answer. If the answer is not entirely complete, describe what exactly I missed." +
            "My answer:\n" +
            answer;
        }

    }
}
