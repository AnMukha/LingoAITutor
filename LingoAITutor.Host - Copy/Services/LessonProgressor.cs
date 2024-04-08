using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Entities.Enums;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace LingoAITutor.Host.Services
{
    public class LessonProgressor
    {
        private readonly LingoDbContext _dbContext;
        private readonly OpenAIAPI _openAPI;

        public LessonProgressor(LingoDbContext dbContext, OpenAIAPI openAPI)
        {
            _dbContext = dbContext;
            _openAPI = openAPI;
        }

        public async Task<Message?> ProgressLesson(Guid lessonId)
        {            
            var lesson = await _dbContext.Lessons.Include(ch => ch.Messages).FirstOrDefaultAsync(ch => ch.LessonId == lessonId);
            if (lesson == null) return null;
            var messages = lesson.Messages.OrderBy(m => m.Number).ToArray();
            if (messages.Last().MessageType == MessageType.GPTMessage)
                return messages.Last();

            var gptResponseText = await GetGPTResponse(lesson, messages);

            lesson.LastMessageNumber++;
            var gptMessage = new Message()
            {
                MessageId = Guid.NewGuid(),
                Content = gptResponseText,
                MessageType = Entities.Enums.MessageType.GPTMessage,
                Number = lesson.LastMessageNumber
            };
            lesson.Messages.Add(gptMessage);
            _dbContext.Add(gptMessage);
            await _dbContext.SaveChangesAsync();
            return gptMessage;
        }

        private async Task<string> GetGPTResponse(Lesson lesson, Message[] messages)
        {
            var result = await _openAPI.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0,
                MaxTokens = 1000,
                Messages = messages.Select(m => 
                        new ChatMessage(m.MessageType == MessageType.GPTMessage? ChatMessageRole.Assistant : ChatMessageRole.User, 
                                        m.Content)).ToList()
            });
            return result.Choices[0].Message.Content;            
        }
    }
}
