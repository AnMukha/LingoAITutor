using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Entities.Enums;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace LingoAITutor.Host.Services
{
    public class TranslationExerciseGenerator
    {
        private readonly LingoDbContext _dbContext;
        private readonly OpenAIAPI _openAPI;
        private static readonly Random _random = new();
        private readonly AllWords _words;

        public TranslationExerciseGenerator(LingoDbContext dbContext, OpenAIAPI api, AllWords words)
        {
            _openAPI = api;
            _dbContext = dbContext;
            _words = words;
        }

        public async Task<WordTranslateExerciseDto> GetNextExercise()
        {
            var (word, strategy) = await FindNextWordToTrain();
            var excercise = await GenerateExerciseForWord(word.Text, strategy);            
            return excercise;
        }

        private NextWordStrategy ChooseStrategy(int num)
        {            
            if (num % 4 == 0)
                return NextWordStrategy.FromTheBestRange;
            if (num % 4 == 1)
                return NextWordStrategy.VocabularyEstimation;
            if (num % 4 == 2)
                return NextWordStrategy.FromFailedWords;
            return NextWordStrategy.CleanUp;
        }

        private async Task<WordTranslateExerciseDto> GenerateExerciseForWord(string word, NextWordStrategy strategy)
        {
            var result = await _openAPI.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 1,
                MaxTokens = 100,
                Messages = new ChatMessage[] {
            new ChatMessage(ChatMessageRole.User, $"I need exercise to improve my English vocabulary. Create an English sentence with the word \"{word}\", translate it to Russian. As output I want only two this sentence in separate lines without any another notices.")
            }
            });

            var resultText = result.Choices[0].Message.Content;

            return new WordTranslateExerciseDto()
            {
                Word = word,
                NativePhrase = resultText.Split('\n').LastOrDefault(),
                OriginalPhrase = resultText.Split('\n').FirstOrDefault(),
                Strategy = strategy
            };
        }

        private async Task<(Word, NextWordStrategy)> FindNextWordToTrain()
        {
            var up = await _dbContext.UserProgresses.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == TranslationExerciseAnaliser.UserId);
            var num = up.ExerciseNumber;
            while (true)
            {
                var strategy = ChooseStrategy(num);
                var result = await FindNextWordToTrain(strategy);
                if (result != null)
                    return (result, strategy);
                num++;
            }            
        }

        private async Task<Word?> FindNextWordToTrain(NextWordStrategy strategy)
        {            
            if (strategy == NextWordStrategy.FromFailedWords)
                return await ChooseFromFailedWords();
            if (strategy == NextWordStrategy.VocabularyEstimation)
                return ChooseFromAllWords();
            if (strategy == NextWordStrategy.FromTheBestRange)
                return await ChooseFromTheBestRange();
            return await CooseForCleanUp();
        }

        private async Task<Word?> CooseForCleanUp()
        {            
            var wordProgress = await _dbContext.UserWordProgresses.Where(up => up.UserID == TranslationExerciseAnaliser.UserId
                                    && up.CorrectUses!= 0).ToDictionaryAsync(up=> up.WordID);
            var word =  _words.GetWords().Where(w => !wordProgress.ContainsKey(w.Id))
                                    .MinBy(w => w.FrequencyRank);
            return word;
        }

        private async Task<Word?> ChooseFromTheBestRange()
        {
            var progress = await _dbContext.RangeProgresses
                                .Where(rp => rp.UserProgressId == TranslationExerciseAnaliser.UserId)     
                                .OrderBy(rp => rp.StartPosition)
                                .AsNoTracking()
                                .ToArrayAsync();            
            // first with poor progress or last
            var chousen = progress.OrderBy(p => p.StartPosition).First(p => p.Progress == null || p.Progress < 0.5);
            chousen ??= progress.Last();
            var position = _random.Next(chousen.WordsCount) + chousen.StartPosition;
            return _words.GetWords()[position]; 
        }

        private Word ChooseFromAllWords()
        {
            return _words.GetWords()[_random.Next(_words.GetWords().Count)];
        }

        private async Task<Word?> ChooseFromFailedWords()
        {
            // the most common word from the list of failed
            var userProgress = await _dbContext.UserWordProgresses.Where(up => up.UserID == TranslationExerciseAnaliser.UserId && up.FailedToUseFlag)
                                .Include(up => up.Word).OrderBy(up => up.Word.FrequencyRank).FirstOrDefaultAsync();
            return userProgress?.Word;

        }
    }
}
