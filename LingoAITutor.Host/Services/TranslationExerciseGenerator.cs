using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Endpoints;
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
        private readonly Guid _userId;

        public TranslationExerciseGenerator(LingoDbContext dbContext, UserIdHepler userIdHelper, OpenAIAPI api, AllWords words)
        {
            _openAPI = api;
            _dbContext = dbContext;
            _words = words;
            _userId = userIdHelper.GetUserId();
        }

        public async Task<WordTranslateExerciseDto> GetNextExercise()
        {
            var (word, up, strategy) = await FindNextWordToTrain();
            var excercise = await GenerateExerciseForWord(word, up, strategy);            
            return excercise;
        }

        private NextWordStrategy ChooseStrategy(int num)
        {
            if (num % 4 == 0)
                return NextWordStrategy.FromFailedWords;
            if (num % 4 == 1)
                return NextWordStrategy.FromTheBestRange;
            if (num % 4 == 2)
                return NextWordStrategy.VocabularyEstimation;            
            return NextWordStrategy.CleanUp;
        }

        string[] Subjects = new string[]
        {
            "Travel", "Home", "Economy", "Hobby", "Rest", "Work", "Science", "Technologies", "Studying", 
            "Family", "Pets", "News", "Policy", "Whether", "Education", "History", "Money"
        };


        private async Task<WordTranslateExerciseDto> GenerateExerciseForWord(Word word, UserWordProgress up, NextWordStrategy strategy)
        {
            var subject = Subjects[_random.Next(Subjects.Length)];
            var sbj = _random.Next(1000) > 750? $" on the subject \"{subject}\"" :"";
            int attempts = 0;
            while (attempts < 3)
            {
                string gptTask;
                if (strategy != NextWordStrategy.FromFailedWords || up == null)
                { 
                    gptTask = $"I need an exercise for A2 level of English to improve my English vocabulary{sbj}. Create one English sentence with the word \"{word.Text}\", translate it to Russian. As output I want only two this sentence in separate lines without any another notices.";
                }
                else
                {
                    gptTask = $"I need an exercise for A2 level of English to improve my English vocabulary{sbj}. I failed to use word \"{word.Text}\" in this sentence: \"{up.FailedToUseSencence}\". Create another English sentence (not same, absolutely different!) with the word \"{word.Text}\" used in a similar sense, translate it to Russian. As output I want only two this sentence in separate lines without any another notices.";
                }
                var result = await _openAPI.Chat.CreateChatCompletionAsync(new ChatRequest()
                {
                    Model = Model.GPT4,
                    Temperature = 1,
                    MaxTokens = 100,
                    Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.User, gptTask)}
                });

                var resultText = result.Choices[0].Message.Content;

                var exercise = new WordTranslateExerciseDto()
                {
                    Word = word.Text,
                    NativePhrase = resultText.Split('\n').LastOrDefault(),
                    OriginalPhrase = resultText.Split('\n').FirstOrDefault(),
                    Strategy = strategy
                };
                if (!string.IsNullOrWhiteSpace(exercise.NativePhrase) && !exercise.NativePhrase.Any(l => l > 'a' && l < 'z'))
                    return exercise;
            }
            throw new Exception("Failed to create exercise.");
        }

        private async Task<(Word, UserWordProgress?, NextWordStrategy)> FindNextWordToTrain()
        {
            var up = await _dbContext.UserProgresses.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == _userId);
            var num = 0;//up.ExerciseNumber;
            if (up?.ExerciseNumber % 5 == 0)
                num = 1;                    
            while (true)
            {
                var strategy = ChooseStrategy(num);
                var result = await FindNextWordToTrain(strategy);
                if (result.word != null)
                    return (result.word, result.up, strategy);
                num++;
            }            
        }

        private async Task<(Word? word, UserWordProgress? up)> FindNextWordToTrain(NextWordStrategy strategy)
        {            
            if (strategy == NextWordStrategy.FromFailedWords)
            {
                var up = await ChooseFromFailedWords();
                return (up?.Word, up);
            }                
            if (strategy == NextWordStrategy.VocabularyEstimation)
                return (ChooseFromAllWords(), null);
            if (strategy == NextWordStrategy.FromTheBestRange)
                return (await ChooseFromTheBestRange(), null);
            return (await CooseForCleanUp(), null);
        }

        private async Task<Word?> CooseForCleanUp()
        {            
            var wordProgress = await _dbContext.UserWordProgresses.Where(up => up.UserID == _userId
                                    && up.CorrectUses!= 0).ToDictionaryAsync(up=> up.WordID);
            var word =  _words.GetWords().Where(w => !wordProgress.ContainsKey(w.Id))
                                    .MinBy(w => w.FrequencyRank);
            return word;
        }

        private async Task<Word?> ChooseFromTheBestRange()
        {
            return _words.GetWords()[_random.Next(1000)];

            var progress = await _dbContext.RangeProgresses
                                .Where(rp => rp.UserProgressId == _userId)     
                                .OrderBy(rp => rp.StartPosition)
                                .AsNoTracking()
                                .ToArrayAsync();
            // first with poor progress or last
            var chousen = progress.OrderBy(p => p.StartPosition).ToArray()[3];//.First(p => p.Progress == null || p.Progress < 0.5);
            chousen ??= progress.Last();
            var position = _random.Next(chousen.WordsCount) + chousen.StartPosition;
            return _words.GetWords()[position]; 
        }

        private Word ChooseFromAllWords()
        {
            return _words.GetWords()[_random.Next(_words.GetWords().Count)];
        }

        private async Task<UserWordProgress?> ChooseFromFailedWords()
        {
            // the most common word from the list of failed
            var userProgress = await _dbContext.UserWordProgresses.Where(up => up.UserID == _userId && up.FailedToUseFlag)
                                .Include(up => up.Word).OrderBy(up => up.Word.FrequencyRank).FirstOrDefaultAsync();
            return userProgress;

        }
    }
}
