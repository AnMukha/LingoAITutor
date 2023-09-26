using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Services
{
    public class VocabularyMapGenerator
    {
        private readonly LingoDbContext _dbContext;

        public VocabularyMapGenerator(LingoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WordProgressDto[]> GetMap()
        {
            var userId = TranslationExerciseAnaliser.UserId;
            var words = await _dbContext.Words.ToArrayAsync();
            var progress = await _dbContext.UserWordProgresses.Where(p => p.User.Id == userId).ToDictionaryAsync(p => p.WordID);
            var result = words.Select(w => MapToWordProgress(w, progress.GetValueOrDefault(w.Id))).ToArray();
            return result;
        }

        private WordProgressDto MapToWordProgress(Word word, UserWordProgress? progress)
        {
            return new WordProgressDto()
            {
                Progress = progress?.CorrectUses ?? 0,
                Wrd = word.Text,
                X = word.XOnMap,
                Y = word.YOnMap
            };
        }

    }
}
