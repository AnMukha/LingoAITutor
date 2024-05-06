using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Services.Vocabulary
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
            var usedCount = await _dbContext.UserWordProgresses.Where(up => up.UserID == userId).CountAsync();
            var usedCorrectly = await _dbContext.UserWordProgresses.Where(up => up.UserID == userId &&
                                up.NonUses < up.CorrectUses).CountAsync();
            var userProgress = _dbContext.UserProgresses == null ? null : await _dbContext.UserProgresses.FirstAsync(u => u.UserId == userId);
            return new VocabularySizeInfoDto()
            {
                EstimatedVocabulary = 0,
                Exercises = userProgress?.ExerciseNumber ?? 0,
                UsedCorrecty = usedCorrectly,
                Used = usedCount
            };
        }
    }
}
