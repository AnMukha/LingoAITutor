using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
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
                Summary = "Get next excercise for vocabulary train",
            });
            application.MapPost("api/voc-train-submit", SubmitAnswer).WithOpenApi(operation => new(operation)
            {
                Summary = "Submit excercise answer for vocabulary train",
            });
        }

        private async static Task<IResult> GetNextExercise(LingoDbContext dbcontext, OpenAIAPI api)
        {
            var word = await FindNextWordToTrain(dbcontext);
            var excercise = await GenerateExerciseForWord(api, word);
            return Results.Ok(excercise);
        }

        private async static Task<WordTranslateExerciseDto> GenerateExerciseForWord(OpenAIAPI api, string word)
        {            
            var result = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.1,
                MaxTokens = 50,
                Messages = new ChatMessage[] {
            new ChatMessage(ChatMessageRole.User, $"Create phrase with word \"{word}\", translate it to Russian, write them only splitted by #.")
            }
            });

            var resultText = result.Choices.First().Message.Content;

            return new WordTranslateExerciseDto()
            {
                Word = word,
                NativePhrase = resultText.Split('#').LastOrDefault(),
                OriginalPhrase = resultText.Split('#').FirstOrDefault(),
            };            
        }

        private async static Task<string> FindNextWordToTrain(LingoDbContext dbcontext)
        {
            var words = await dbcontext.Words.ToArrayAsync();
            return words[random.Next(words.Length-1)].Text;
        }

        private async static Task<IResult> SubmitAnswer(LingoDbContext dbcontext, OpenAIAPI api, [FromBody] PhraseDto phrase)
        {
            var result = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.1,
                MaxTokens = 50,
                Messages = new ChatMessage[] {
            new ChatMessage(ChatMessageRole.User, $"Fix grammar errors in the phrase: \"{phrase.Text}\". Write fixed version, then write delimiter # and then explain the errors.")
            }
            });

            var resultText = result.Choices.First().Message.Content;

            return Results.Ok(new WordTranslateFeedback() 
            {
                FixedPhrase = resultText.Split('#').FirstOrDefault(),
                Feedback = resultText.Split('#').LastOrDefault()
            });
        }

    }
}

