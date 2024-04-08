using LingoAITutor.Host.Entities.Enums;
using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Services.Interfaces;
using OpenAI_API.Chat;
using OpenAI_API;
using Microsoft.EntityFrameworkCore;
using OpenAI_API.Models;

namespace LingoAITutor.Host.Services.LessonProgress
{
    public class FreeChatProgressor: ILessonProgressor
    {
        private readonly LingoDbContext _dbContext;
        private readonly OpenAIAPI _openAPI;

        public FreeChatProgressor(LingoDbContext dbContext, OpenAIAPI openAPI)
        {
            _dbContext = dbContext;
            _openAPI = openAPI;
        }

        public async Task<Message?> ProgressLesson(Lesson lesson, ScenarioTemplate scenario)
        {
            var lessonWithMessages = await _dbContext.Lessons.Include(ch => ch.Messages).FirstOrDefaultAsync(ch => ch.LessonId == lesson.LessonId);
            if (lessonWithMessages == null) return null;
            var messages = lessonWithMessages.Messages.OrderBy(m => m.Number).ToArray();
            if (messages.Last().MessageType == MessageType.GPTMessage)
                return messages.Last();

            var gptResponseText = await GetGPTResponse(messages);

            lessonWithMessages.LastMessageNumber++;
            lessonWithMessages.MessagesCount++;
            var gptMessage = new Message()
            {
                MessageId = Guid.NewGuid(),
                Content = gptResponseText,
                MessageType = MessageType.GPTMessage,
                Number = lessonWithMessages.LastMessageNumber,
                SectionNumber = lessonWithMessages.SectionNumber
            };
            lessonWithMessages.Messages.Add(gptMessage);
            _dbContext.Add(gptMessage);
            await _dbContext.SaveChangesAsync();
            return gptMessage;
        }

        private async Task<string> GetGPTResponse(Message[] messages)
        {
            var result = await _openAPI.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0,
                MaxTokens = 1000,
                Messages = messages.Select(m =>
                        new ChatMessage(m.MessageType == MessageType.GPTMessage ? ChatMessageRole.Assistant : ChatMessageRole.User,
                                        m.Content)).ToList()
            });
            return result.Choices[0].Message.Content;
        }
    }
}
