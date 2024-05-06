using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace LingoAITutor.Host.Services.Translation
{
    public class SentenceTranslator
    {
        private readonly OpenAIAPI _openAPI;

        public SentenceTranslator(OpenAIAPI openAPI)
        {
            _openAPI = openAPI;
        }

        public async Task<string> Translate(string sentence, string sourceLanguage, string targetLanguage)
        {
            var resultText = await _openAPI.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0,
                MaxTokens = 50,
                Messages = new ChatMessage[] { new ChatMessage(ChatMessageRole.User, GetTranslateRequest(sentence, sourceLanguage, targetLanguage)) }
            });
            return  resultText.Choices[0].Message.TextContent;            
        }

        private string GetTranslateRequest(string sentence, string sourceLanguage, string targetLanguage)
        {
            return $"Translate the text from {sourceLanguage} to {targetLanguage}. Return translation without any comments. Text:  \n" + sentence;                   
        }

    }
}
