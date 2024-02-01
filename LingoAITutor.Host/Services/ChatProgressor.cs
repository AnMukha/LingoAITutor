using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Entities.Enums;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace LingoAITutor.Host.Services
{
    public class ChatProgressor
    {
        private readonly LingoDbContext _dbContext;
        private readonly OpenAIAPI _openAPI;

        public ChatProgressor(LingoDbContext dbContext, OpenAIAPI openAPI)
        {
            _dbContext = dbContext;
            _openAPI = openAPI;
        }

        public async Task<Message?> ProgressChat(Guid chatId)
        {            
            var chat = await _dbContext.Chats.Include(ch => ch.Messages).FirstOrDefaultAsync(ch => ch.ChatId == chatId);
            if (chat == null) return null;
            var messages = chat.Messages.OrderBy(m => m.Number).ToArray();
            if (messages.Last().MessageType == Entities.Enums.MessageType.GPTMessage)
                return messages.Last();

            var gptResponseText = await GetGPTResponse(chat, messages);

            chat.LastMessageNumber++;
            var gptMessage = new Message()
            {
                MessageId = Guid.NewGuid(),
                Content = gptResponseText,
                MessageType = Entities.Enums.MessageType.GPTMessage,
                Number = chat.LastMessageNumber
            };
            chat.Messages.Add(gptMessage);
            _dbContext.Add(gptMessage);
            await _dbContext.SaveChangesAsync();
            return gptMessage;
        }

        private async Task<string> GetGPTResponse(Chat chat, Message[] messages)
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
