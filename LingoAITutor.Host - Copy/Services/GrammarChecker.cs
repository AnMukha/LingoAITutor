using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Entities.Enums;
using LingoAITutor.Host.Infrastructure;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace LingoAITutor.Host.Services
{
    public class GrammarChecker
    {        
        private readonly OpenAIAPI _openAPI;

        public GrammarChecker(OpenAIAPI openAPI)
        {            
            _openAPI = openAPI;
        }

        public async Task<string> FixGrammar(string? content)
        {
            var result = await _openAPI.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0,
                MaxTokens = 1000,
                Messages = new ChatMessage[] { new ChatMessage(ChatMessageRole.User, GetGrammarFixRequest(content)) }
            });
            return result.Choices[0].Message.Content;
        }

        private string GetGrammarFixRequest(string? content)
        {
            return "Fix grammar errors in the text. I want to get in response only fixed text without comments. The text: \"" + content + "\"";
        }
    }
}
