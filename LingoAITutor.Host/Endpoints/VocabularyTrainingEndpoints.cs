using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace LingoAITutor.Host.Endpoints
{

    public static class VocabularyTrainingEndpoints
    {
        private static readonly Random random = new Random(999);

        public static void AddEndpoints(WebApplication application)
        {
            application.MapGet("api/voc-train-next", GetNextExercise).WithOpenApi(operation => new(operation)
            {
                Summary = "Get next excercise for vacabulary train",
            });
        }

        private async static Task<IResult> GetNextExercise(LingoDbContext dbcontext)
        {
            var word = await FindNextWordToTrain(dbcontext);
            var excercise = await GenerateExerciseForWord(word);
            return Results.Ok(excercise);
        }

        private async static Task<WordTranslateExerciseDto> GenerateExerciseForWord(string word)
        {

            var api = new OpenAI_API.OpenAIAPI("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");
            var result = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.1,
                MaxTokens = 50,
                Messages = new ChatMessage[] {
            new ChatMessage(ChatMessageRole.User, "Create phrase with word \""+ word+"\", translate it to Russian, write them only splitted by #.")
            }
            }); ;

            return new WordTranslateExerciseDto()
            {
                Word = word,
                NativePhrase = result.Choices.First().Message.Content
            };
        }

        private async static Task<string> FindNextWordToTrain(LingoDbContext dbcontext)
        {
            var words = await dbcontext.Words.ToArrayAsync();
            return words[random.Next(words.Length-1)].Text;
        }
    }
}

