using Azure;
using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Endpoints;
using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Entities.Enums;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Services.Common;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LingoAITutor.Host.Services
{
    public class TranslationExerciseAnaliser
    {
        private readonly LingoDbContext _dbContext;
        private readonly OpenAIAPI _openAPI;
        private readonly AllWords _allWords;
        private readonly Guid _userId;
        private readonly IrregularVerbs _irregularVerbs;

        public TranslationExerciseAnaliser(LingoDbContext dbContext, UserIdHepler userIdHelper,  OpenAIAPI api, AllWords allWords, IrregularVerbs irregularVerbs)
        {
            _openAPI = api;
            _dbContext = dbContext;
            _allWords = allWords;
            _userId = userIdHelper.GetUserId();
            _irregularVerbs = irregularVerbs;
        }

        public async Task<WordTranslateFeedback> AnalyseAnswer(AnswerDto answer)
        {
            var result = await _openAPI.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0,
                MaxTokens = 500,
                Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.User,
                    "Correct rude grammar errors in my translation from Russian to English. Explain why you corrected the errors and what rules of grammar were broken. "+
                    $"Original sentence: \"{answer.ExerciseText}\"/n"+
                    $"My translation: \"{answer.AnswerText}\"/n"+
                    "Write results in the format:"+
                    "Corrected sentence: {corrected sentence here}"+
                    "Explanations: {Errors explanations}"
                    )
            }
            });

            var resultText = result.Choices[0].Message.Content;

            var fixedPhrase = resultText.Split('\n').FirstOrDefault()![21..];
            if (fixedPhrase.StartsWith('"'))
                fixedPhrase = fixedPhrase[1..];
            if (fixedPhrase.EndsWith('"'))
                fixedPhrase = fixedPhrase[0..^1];
            var explonations = string.Join("\n", resultText.Split('\n').Skip(2));

            await IncreaseExerciseNumber();

            var (badMatchig, correctly, incorrectly, answerWords) = AnalyzeUsedWords(answer.AnswerText!, fixedPhrase);
            var original = ExtractOriginalWords(answer.OriginalPhrase!);
            if (!badMatchig)
            {
                await UpdateWordsProgress(correctly, incorrectly, answerWords, original,  answer.Word!, fixedPhrase);
                if (answer.Strategy == NextWordStrategy.VocabularyEstimation || answer.Strategy == NextWordStrategy.FromTheBestRange)
                {
                    var successUsing = await GetMainWordCorrectUse(answer, fixedPhrase);
                    if (successUsing != null)
                    {
                        var num = await IncreaseVocabularyEstimationNumber();
                        await SaveEstimationResult(answer.Word!, num, successUsing.Value);
                        if (num % 10 == 0)
                            await RecalculateVocabularyEstimation(num);
                    }
                }
            }
            else
            {
                await MarkMainWordIfUsed(answerWords, answer.Word!);
            }
            return new WordTranslateFeedback()
            {
                FixedPhrase = fixedPhrase,
                Feedback = explonations
            };
        }

        private async Task MarkMainWordIfUsed(string[] answerWords, string wordText)
        {
            var word = FindWord(wordText);
            if (answerWords.Any(w => IsSameWord(w, wordText)))
            {                
                var progress = await GetOrCreateProgress(word!.Id);
                progress.FailedToUseFlag = false;
                _dbContext.SaveChanges();
            }
        }

        private string[] ExtractOriginalWords(string originalPhrase)
        {
            return Regex.Split(originalPhrase, @"\W+").Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();
        }

        private async Task RecalculateVocabularyEstimation(int num)
        {
            return;
            var estimatedRecently = await _dbContext.UserWordProgresses.Where(up =>
                                    up.UserID == _userId &&
                                    up.EstimationExerciseNumber != null &&
                                    up.EstimationExerciseNumber > num - 200).AsNoTracking().ToArrayAsync();
            // вычислить процент известных слов для каждой зоны
            var ranges = await _dbContext.RangeProgresses.Where(rp => rp.UserProgressId == _userId).ToArrayAsync();
            foreach(var r in ranges)
            {
                var inRange = estimatedRecently.Where(er => er.Word.FrequencyRank > r.StartPosition && er.Word.FrequencyRank < r.StartPosition + r.WordsCount).ToArray();
                if (inRange.Length > 10)
                    r.Progress = inRange.Count(wp => wp.EstimationExerciseResult == true) / (double)inRange.Length;
            }
            _dbContext.SaveChanges();
        }

        private async Task SaveEstimationResult(string wordText, int estimationNumber, bool successUsing)
        {
            var word = _allWords.FindWordByText(wordText);
            if (word == null)
                return;
            var wordProgress = await _dbContext.UserWordProgresses.FirstOrDefaultAsync(
                            w => w.UserID == _userId && w.WordID == word.Id);
            if (wordProgress == null)
            {                
                wordProgress = new()
                {
                    Id = Guid.NewGuid(),
                    UserID = _userId,
                    WordID = word.Id
                };
                _dbContext.UserWordProgresses.Add(wordProgress);
            }
            wordProgress.EstimationExerciseNumber = estimationNumber;
            wordProgress.EstimationExerciseResult = successUsing;
            _dbContext.SaveChanges();
        }

        private async Task<bool?> GetMainWordCorrectUse(AnswerDto answer, string fixedPhrase)
        {            
            var answerWords = Regex.Split(answer.AnswerText!, @"\W+").Where(w => !string.IsNullOrWhiteSpace(w) && NotExcludedFromAnalysis(w)).ToArray();
            var fixedWords = Regex.Split(fixedPhrase, @"\W+").Where(w => !string.IsNullOrWhiteSpace(w) && NotExcludedFromAnalysis(w)).ToArray();
            // корректно: слово есть в переводе
            if (answerWords.Any(w => IsSameWord(w, answer.Word!)))
                return true;
            // слово есть в исправленном, нет в переводе - не корректно.
            if (fixedWords.Any(w => IsSameWord(w, answer.Word!)))
                return false;
            // ни там ни там - спросить чат
            var result = await _openAPI.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0,
                MaxTokens = 500,
                Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.User,
                    "These are two same sentences:\n"+
                    $"1. {fixedPhrase}"+
                    $"2. {answer.ExerciseText}"+
                    $"Which word in the first sentence is used instead of \"{answer.Word}\" in the second sentence?/n"+
                    "I need short response with the word only."
            )}
            });
            var resultText = result.Choices[0].Message.Content;
            resultText = resultText.Replace("\"", "");
            if (Regex.Split(resultText, @"\W+").Length == 1)
            {
                // соответствующее слово переведено не верно
                if (!answerWords.Any(w => IsSameWord(w, resultText)))
                    return false;                
            }
            return null;
        }

        private async Task UpdateWordsProgress(string[]? correctly, string[]? incorrectly, string[] answerWords, string[] original, string exerciseWordText, string fixedSentence)
        {
            var updates = (correctly ?? Array.Empty<string>()).Select(w => (word: w, correct: true))
                            .Concat((incorrectly ?? Array.Empty<string>()).Select(w => (word: w, correct: false)));
            foreach (var (wordText, correct) in updates)
            {
                var word = FindWord(wordText);
                if (word != null)
                {
                    var progress = await GetOrCreateProgress(word.Id);
                    if (correct)
                    {
                        progress.CorrectUses++;
                        //if (IsSameWord(word, exerciseWord))
                        progress.FailedToUseFlag = false;
                    }
                    else
                    {
                        progress.NonUses++;
                        if (progress.NonUses >= progress.CorrectUses && word.FrequencyRank >= 300 )// && !original.Any(w => IsSameWord(wordText, w)))
                            progress.FailedToUseFlag = true;
                        progress.FailedToUseSencence = fixedSentence;
                    }
                }                
            }
            var exerciseWord = FindWord(exerciseWordText);
            if (answerWords.Any(w => IsSameWord(w, exerciseWordText)))
            {
                var progress = await GetOrCreateProgress(exerciseWord!.Id);
                progress.FailedToUseFlag = false;                
            }
            _dbContext.SaveChanges();
        }

        private async Task<UserWordProgress> GetOrCreateProgress(Guid wordId)
        {
            var progress = await _dbContext.UserWordProgresses.FirstOrDefaultAsync(p => p.WordID == wordId && p.UserID == _userId);
            if (progress == null)
            {
                progress = new()
                {
                    Id = Guid.NewGuid(),
                    UserID = _userId,
                    WordID = wordId
                };
                _dbContext.UserWordProgresses.Add(progress);
            }
            return progress;
        }

        private async Task IncreaseExerciseNumber()
        {
            var progress = await _dbContext.UserProgresses.FirstOrDefaultAsync(r => r.UserId == _userId);
            if (progress == null)
                return;
            progress!.ExerciseNumber++;
            await _dbContext.SaveChangesAsync();
        }

        private async Task<int> IncreaseVocabularyEstimationNumber()
        {
            var progress = await _dbContext.UserProgresses.FirstOrDefaultAsync(r => r.UserId == _userId);
            if (progress == null)
                return 0;
            progress!.EstimationNumber = progress.EstimationNumber + 1;
            _dbContext.SaveChanges();
            return progress!.EstimationNumber;
        }


        private (bool badMatchig, string[]? correctly, string[]? incorrectly, string[] answerWords) AnalyzeUsedWords(string answerText, string fixedText)
        {
            var answerWords = Regex.Split(answerText, @"\W+").Where(w => !string.IsNullOrWhiteSpace(w) && NotExcludedFromAnalysis(w)).ToArray();
            var fixedWords = Regex.Split(fixedText, @"\W+").Where(w => !string.IsNullOrWhiteSpace(w) && NotExcludedFromAnalysis(w)).ToArray();            
            var correctly = answerWords.Where(w => fixedWords.Any(fw => IsSameWord(fw, w))).ToArray();
            var incorrectly = fixedWords.Where(w => !answerWords.Any(aw => IsSameWord(aw, w))).ToArray();
            return (BadMatching(answerWords, fixedWords), correctly, incorrectly, answerWords);
        }

        private Word? FindWord(string wordText)
        {
            var word = _allWords.FindWordByText(wordText.ToLower());            
            if (word is not null)
                return word;
            var same = _allWords.GetWords().FirstOrDefault(w => IsSameWord(w.Text, wordText));
            if (same is not null)
                return same;
            return null;
        }

        private bool BadMatching(string[] answerWords, string[] fixedWords)
        {            
            if (answerWords.Where(w => fixedWords.Any(fw => IsSameWord(w, fw))).Count() / (double)fixedWords.Length < 0.5)
                return true;
            if (answerWords.Where(NotExcludedFromAnalysis).Count() / (double)fixedWords.Where(NotExcludedFromAnalysis).Count() < 0.75)
                return true;
            return false;
        }

        private bool NotExcludedFromAnalysis(string w)
        {
            // можно пропусткать артикли и путать предлоги, употреблять неверные модальные глаголы - это не относится к словарному запасу
            return !SpecialWords.NotAnalizedWords.Contains(w.ToLower());
        }

        public static bool IsSameWord(string w1, string w2, IrregularVerbs? iv = null)
        {
            var wl1 = w1.ToLower();
            var wl2 = w2.ToLower();
            if (wl1 == wl2) return true;
            var n1 = NormalizeWord(wl1, iv);
            var n2 = NormalizeWord(wl2, iv);
            return (n1 == n2 || n1 + "e" == n2 || n1 == n2 + "e");            
        }

        private static string NormalizeWord(string w, IrregularVerbs? iv)
        {
            if (iv is not null)
            {
                var v1 = iv.FindFirstForm(w);
                if (v1 != null) return v1;
            }
            if (w.EndsWith("ies")) return w[..^3] + "y";
            if (w.EndsWith("es")) return w[..^2];
            if (w.EndsWith("s") && w.Length>1 && w[w.Length - 2] != 's' && w[w.Length - 2] != 'h' && w[w.Length - 2] != 'x') return w[..^1];
            if (w.EndsWith("ed")) return w[..^2];            
            
            if (w.EndsWith("ing"))
            {
                if (w.Length > 4 && w[w.Length-4] == w[w.Length-5])
                    return w[..^4];
                return w[..^3];
            }
            return w;
        }
    }
}
