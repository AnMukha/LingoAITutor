﻿using Azure;
using LingoAITutor.Host.Dto;
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


        public readonly static Guid UserId = new("5944D4A0-0D55-402B-B247-42E6765B3410");

        public TranslationExerciseAnaliser(LingoDbContext dbContext, OpenAIAPI api, AllWords allWords)
        {
            _openAPI = api;
            _dbContext = dbContext;
            _allWords = allWords;
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
                    "Correct grammar errors in my translation from Russian to English. Explain why you corrected the errors and what rules of grammar were broken. "+
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

            var (badMatchig, correctly, incorrectly) = AnalyzeUsedWords(answer.AnswerText!, fixedPhrase);
            if (!badMatchig)
            {
                await UpdateWordsProgress(correctly, incorrectly, answer.Word);
                if (answer.Strategy == NextWordStrategy.VocabularyEstimation || answer.Strategy == NextWordStrategy.FromTheBestRange)
                {
                    var successUsing = await GetMainWordCorrectUse(answer, fixedPhrase);
                    if (successUsing != null)
                    {
                        var num = await IncreaseVocabularyEstimationNumber();
                        await SaveEstimationResult(answer.Word!, num, successUsing.Value);
                    }
                }
            }
            return new WordTranslateFeedback()
            {
                FixedPhrase = fixedPhrase,
                Feedback = explonations
            };
        }

        private async Task SaveEstimationResult(string wordText, int estimationNumber, bool successUsing)
        {
            var word = _allWords.FindWordByText(wordText);
            if (word == null)
                return;
            var wordProgress = await _dbContext.UserWordProgresses.FirstOrDefaultAsync(
                            w => w.UserID == TranslationExerciseAnaliser.UserId && w.WordID == word.Id);
            if (wordProgress == null)
            {                
                wordProgress = new()
                {
                    Id = Guid.NewGuid(),
                    UserID = UserId,
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
            var answerWords = Regex.Split(answer.AnswerText!, @"\W+").Where(w => !string.IsNullOrWhiteSpace(w) && NotGrammarWord(w)).ToArray();
            var fixedWords = Regex.Split(fixedPhrase, @"\W+").Where(w => !string.IsNullOrWhiteSpace(w) && NotGrammarWord(w)).ToArray();
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
            resultText = resultText.Remove('"');
            if (Regex.Split(resultText, @"\W+").Length == 1)
            {
                // соответствующее слово переведено не верно
                if (!answerWords.Any(w => IsSameWord(w, resultText)))
                    return false;                
            }
            return null;
        }

        private async Task UpdateWordsProgress(string[]? correctly, string[]? incorrectly, string exerciseWord)
        {
            var updates = (correctly ?? Array.Empty<string>()).Select(w => (word: w, correct: true))
                            .Concat((incorrectly ?? Array.Empty<string>()).Select(w => (word: w, correct: false)));
            foreach (var (word, correct) in updates)
            {
                var wordId = FindWordId(word);
                if (wordId != null)
                {
                    var progress = await _dbContext.UserWordProgresses.FirstOrDefaultAsync(p => p.WordID == wordId.Value && p.UserID == UserId);
                    if (progress == null)
                    {
                        progress = new()
                        {
                            Id = Guid.NewGuid(),
                            UserID = UserId,
                            WordID = wordId.Value
                        };
                        _dbContext.UserWordProgresses.Add(progress);
                    }
                    if (correct)
                    {
                        progress.CorrectUses++;
                        if (IsSameWord(word, exerciseWord))
                            progress.FailedToUseFlag = false;
                    }
                    else
                    {
                        progress.NonUses++;
                        progress.FailedToUseFlag = true;
                    }                    
                }                
            }            
            _dbContext.SaveChanges();
        }

        private async Task IncreaseExerciseNumber()
        {
            var progress = await _dbContext.UserProgresses.FirstOrDefaultAsync(r => r.UserId == TranslationExerciseAnaliser.UserId);
            progress!.ExerciseNumber++;
            await _dbContext.SaveChangesAsync();
        }

        private async Task<int> IncreaseVocabularyEstimationNumber()
        {
            var progress = await _dbContext.UserProgresses.FirstOrDefaultAsync(r => r.UserId == TranslationExerciseAnaliser.UserId);
            progress!.EstimationNumber = progress.EstimationNumber + 1;
            _dbContext.SaveChanges();
            return progress!.EstimationNumber;
        }


        private (bool badMatchig, string[]? correctly, string[]? incorrectly) AnalyzeUsedWords(string answerText, string fixedText)
        {
            var answerWords = Regex.Split(answerText, @"\W+").Where(w => !string.IsNullOrWhiteSpace(w) && NotGrammarWord(w)).ToArray();
            var fixedWords = Regex.Split(fixedText, @"\W+").Where(w => !string.IsNullOrWhiteSpace(w) && NotGrammarWord(w)).ToArray();
            // if it is too bad, do not take into account this exercise
            if (BadMatching(answerWords, fixedWords))
                return (true, null, null);
            var correctly = answerWords.Where(w => fixedWords.Any(fw => IsSameWord(fw, w))).ToArray();
            var incorrectly = fixedWords.Where(w => !answerWords.Any(aw => IsSameWord(aw, w))).ToArray();
            return (false, correctly, incorrectly);
        }

        private Guid? FindWordId(string wordText)
        {
            var word = _allWords.FindWordByText(wordText.ToLower());            
            if (word is not null)
                return word.Id;
            var same = _allWords.GetWords().FirstOrDefault(w => IsSameWord(w.Text, wordText));
            if (same is not null)
                return same.Id;
            return null;
        }

        private bool BadMatching(string[] answerWords, string[] fixedWords)
        {
            if (answerWords.Where(w => fixedWords.Any(fw => IsSameWord(w, fw))).Count() / (double)fixedWords.Length < 0.5)
                return true;
            if (answerWords.Where(NotGrammarWord).Count() / (double)fixedWords.Where(NotGrammarWord).Count() < 0.8)
                return true;
            return false;
        }

        private bool NotGrammarWord(string w)
        {
            // можно пропусткать артикли и путать предлоги, употреблять неверные модальные глаголы - это не относится к словарному запасу
            return !SpecialWords.GrammarWords.Contains(w.ToLower());
        }

        private static bool IsSameWord(string w1, string w2)
        {
            var wl1 = w1.ToLower();
            var wl2 = w2.ToLower();
            if (wl1 == wl2) return true;
            return NormalizeWord(wl1) == NormalizeWord(wl2);
            
            /*return w1 ==
            if (w1 == w2) return true;
            if (w1 + "s" == w2 || w2 + "s" == w1) return true;
            if (w1 + "es" == w2 || w2 + "es" == w1) return true;
            if (w1[..^1] + "ies" == w2 || w2[..^1] + "ies" == w1) return true;
            if (w1 + "d" == w2 || w2 + "d" == w1) return true;
            if (w1 + "ed" == w2 || w2 + "ed" == w1) return true;            
            if (w1 + "ing" == w2 || w2 + "ing" == w1) return true;
            if (w1 + w1[w1.Length - 1] + "ing" == w2 || w2 + w2[w2.Length - 1] + "ing" == w1) return true;
            if (w1.Length > 1 && w1.EndsWith('e'))
            {
                if (w1[..^1] + w1[w1.Length - 2] + "ing" == w2) return true;
                if (w1[..^1] + "ing" == w2) return true;
            }
            if (w2.Length > 1 && w2.EndsWith('e'))
            {
                if (w2[..^1] + w2[w2.Length - 2] + "ing" == w1) return true;
                if (w2[..^1] + "ing" == w1) return true;
            }
            return false;*/
        }

        private static string NormalizeWord(string w)
        {
            if (w.EndsWith("es")) return w[..^2];
            if (w.EndsWith("s")) return w[..^1];
            if (w.EndsWith("ed")) return w[..^2];
            if (w.EndsWith("d")) return w[..^1];
            if (w.EndsWith("ies")) return w[..^3]+"y";
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