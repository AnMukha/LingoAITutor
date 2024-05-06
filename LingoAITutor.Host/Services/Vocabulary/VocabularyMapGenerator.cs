using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Services.Vocabulary
{
    public class VocabularyMapGenerator
    {
        private readonly LingoDbContext _dbContext;

        public VocabularyMapGenerator(LingoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WordProgressDto[]> GetMap(Guid userId)
        {
            var words = await _dbContext.Words.ToArrayAsync();
            var progress = await _dbContext.UserWordProgresses.Where(p => p.User.Id == userId).ToArrayAsync();
            var progressDict = progress.DistinctBy(p => p.WordID).ToDictionary(p => p.WordID);
            var result = words.Select(w => MapToWordProgress(w, progressDict.GetValueOrDefault(w.Id))).ToArray();
            return result;
        }

        private WordProgressDto MapToWordProgress(Word word, UserWordProgress? progress)
        {
            return new WordProgressDto()
            {
                Wrd = word.Text,
                X = word.XOnMap,
                Y = word.YOnMap,
                CorrectUses = progress?.CorrectUses ?? 0,
                NonUses = progress?.NonUses ?? 0,
                Failed = progress?.FailedToUseFlag ?? false
            };
        }

    }
}
