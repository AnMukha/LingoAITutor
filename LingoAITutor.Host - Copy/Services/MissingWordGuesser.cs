using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace LingoAITutor.Host.Services
{
    public class MissingWordGuesser
    {
        private readonly OpenAIAPI _openAPI;
        public MissingWordGuesser(OpenAIAPI openAPI)
        {
            _openAPI = openAPI;
        }
        public async Task<string[]> GuessMissingWord(string text, int missingWordPosition)
        {
            var resultText = await _openAPI.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0,
                MaxTokens = 50,
                Messages = new ChatMessage[] { new ChatMessage(ChatMessageRole.User, GetWordPredictionRequest(text, missingWordPosition)) }
            });
            var result = resultText.Choices[0].Message.TextContent.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (result.Length == 1 && result[0].Contains(','))
            {
                result = result[0].Split(",").Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToArray();
                return result;
            }
            result = result.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s =>
                {
                    var pntPos = s.IndexOf('.');
                    if (pntPos == -1)
                        return s;
                    return s.Substring(pntPos+1).TrimStart();
                }).Distinct().ToArray();
            return result;
        }

        private string GetWordPredictionRequest(string text, int missingWordPosition)
        {
            return "Guess the missing word in this text:\r\n" +
                   text.Substring(0, missingWordPosition) + " (missing word) " + text.Substring(missingWordPosition)+ "\r\n" +
                   "Return a few options, I need words only, without comments.";            
        }
    }
}
