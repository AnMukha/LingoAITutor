using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using OpenAI_API.Moderation;
using System.Text.RegularExpressions;

namespace LingoAITutor.Host.Endpoints
{

    public static class VocabularyTrainingEndpoints
    {
        private static readonly Random random = new Random();

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

        private static Word[] AllWords = null!;

        private static async Task<Word[]> GetWords(LingoDbContext dbcontext)
        {
            return await dbcontext.Words.AsNoTracking().ToArrayAsync();
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
                MaxTokens = 100,
                Messages = new ChatMessage[] {
            new ChatMessage(ChatMessageRole.User, $"I need exercise to improve my English vocabulary. Create an English sentence with the word \"{word}\", translate it to Russian. As output I want only two this sentence in separate lines without any another notices.")
            }
            });

            var resultText = result.Choices.First().Message.Content;

            return new WordTranslateExerciseDto()
            {
                Word = word,
                NativePhrase = resultText.Split('\n').LastOrDefault(),
                OriginalPhrase = resultText.Split('\n').FirstOrDefault(),
            };            
        }

        private async static Task<string> FindNextWordToTrain(LingoDbContext dbcontext)
        {
            var words = await dbcontext.Words.ToArrayAsync();
            return words[words.Length - random.Next(1000)].Text;
        }

        private async static Task<IResult> SubmitAnswer(LingoDbContext dbcontext, OpenAIAPI api, [FromBody] AnswerDto answer)
        {
            var result = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.1,
                MaxTokens = 500,               
                Messages = new ChatMessage[] {                    
                    new ChatMessage(ChatMessageRole.User,
                    "Correct grammar errors in my translation from Russian to English. Explain why you corrected the errors and what rules of grammar were broken. "+                    
                    $"Original sentence: \"{answer.ExerciseText}\"/n"+
                    $"My translation: \"{answer.AnswerText}\"/n"+
                    "Write results in the format:"+
                    "Corrected sentence: {corrected sentence here}"+
                    "Explanations: {Errors explanations}"
                    )
            }
            });

            var resultText = result.Choices.First().Message.Content;            

            var fixedPhrase = resultText.Split('\n').FirstOrDefault()!.Substring(21);
            if (fixedPhrase.StartsWith('"'))
                fixedPhrase = fixedPhrase.Substring(1, fixedPhrase.Length - 2);
            var explonations = string.Join("\n", resultText.Split('\n').Skip(2));

            await AnalyseVocabulary(answer.AnswerText!, fixedPhrase, dbcontext);

            return Results.Ok(new WordTranslateFeedback() 
            {
                FixedPhrase = fixedPhrase,
                Feedback = explonations
            });
        }

        public static Guid UserId = new Guid("5944D4A0-0D55-402B-B247-42E6765B3410");

        private static async Task AnalyseVocabulary(string answerText, string fixedText, LingoDbContext dbcontext)
        {
            var answerWords = Regex.Split(answerText, @"\W+").ToArray();
            var fixedWords = Regex.Split(fixedText, @"\W+").ToArray();
            // if it is too bad, do not take into account this exercise
            if (BadMatching(answerWords, fixedWords))
                return;
            // for each word in result text find word in phrase, if it exists mark word as used.
            var correctlyUsed = answerWords.Where(w =>  IsNotArticle(w) && fixedWords.Any(fw => IsSameWord(fw, w))).ToArray();
            foreach (var w in correctlyUsed)
            {
                var wordId = await FindWordId(w, dbcontext);
                if (wordId != null)
                {
                    var progress = await dbcontext.UserWordProgresses.FirstOrDefaultAsync(p => p.WordID == wordId.Value && p.UserID == UserId);
                    if (progress == null)
                    {
                        progress = new UserWordProgress();
                        progress.Id = Guid.NewGuid();
                        progress.UserID = UserId;
                        progress.WordID = wordId.Value;
                        dbcontext.UserWordProgresses.Add(progress);
                    }
                    progress.MasteryLevel = progress.MasteryLevel + 1;
                }
            }
            dbcontext.SaveChanges();
        }

        private static async Task<Guid?> FindWordId(string word, LingoDbContext dbContext)
        {
            var words = await GetWords(dbContext);
            var equal = words.FirstOrDefault(w => w.Text == word);
            if (equal is not null)
                return equal.Id;
            var same = words.FirstOrDefault(w => IsSameWord(w.Text, word));
            if (same is not null)
                return same.Id;
            return null;
        }

        private static bool BadMatching(string[] answerWords, string[] fixedWords)
        {
            if (answerWords.Where(w => fixedWords.Any(fw => IsSameWord(w, fw))).Count() / (double)fixedWords.Count() < 0.5)
                return true;            
            if (answerWords.Where(IsNotArticle).Count() / (double)fixedWords.Where(IsNotArticle).Count() < 0.8)
                return true;
            return false;
        }

        private static bool IsNotArticle(string w)
        {
            return w.ToLower() != "the" && w.ToLower()!="a";
        }

        private static bool IsSameWord(string w, string fw)
        {
            var w1 = w.ToLower();
            var w2 = fw.ToLower();
            if (w1 == w2) return true;
            if (w1 + "s" == w2 || w2 + "s" == w1) return true;
            if (w1 + "es" == w2 || w2 + "es" == w1) return true;
            if (w1 + "d" == w2 || w2 + "d" == w1) return true;
            if (w1 + "ed" == w2 || w2 + "ed" == w1) return true;
            return false;
        }

        private static Task UpdateVocaluaryData(object analysisResult)
        {
            throw new NotImplementedException();
        }

        private static object ParseAnalysisResult(ChatResult analysisText)
        {
            throw new NotImplementedException();
        }
    }
}

