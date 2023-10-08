﻿using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Services
{
    public class VocabularySizeCalculation
    {
        private readonly LingoDbContext _dbContext;
        private readonly AllWords _words;
        public VocabularySizeCalculation(LingoDbContext dbContext, AllWords words)
        {
            _dbContext = dbContext;
            _words = words;
        }

        public async Task<VocabularySizeInfoDto> CalculateVocabularySize(Guid userId)
        {            
            var ranges = await _dbContext.RangeProgresses.Where(rp => rp.UserProgressId == userId).AsNoTracking().ToArrayAsync();
            var estimated = ranges.Where(r => r.Progress.HasValue)
                                            .Select(r => r.Progress!.Value * _words.GetCountInRange(r.StartPosition, r.WordsCount))
                                            .Sum();
            var usedCount = await _dbContext.UserWordProgresses.Where(up => up.UserID == userId).CountAsync();
            var usedCorrectly = await _dbContext.UserWordProgresses.Where(up => up.UserID == userId &&
                                up.NonUses < up.CorrectUses).CountAsync();
            var userProgress = await _dbContext.UserProgresses.FirstAsync(u => u.UserId == userId);
            return new VocabularySizeInfoDto()
            {
                EstimatedVocabulary = (int)Math.Round(estimated),
                Exercises = userProgress.ExerciseNumber,
                UsedCorrecty = usedCorrectly,
                Used = usedCount
            };            
        }
    }
}
