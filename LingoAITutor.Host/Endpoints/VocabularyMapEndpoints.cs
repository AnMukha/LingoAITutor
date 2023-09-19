using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Endpoints
{    
    public static class VocabularyMapEndpoints
    {
        public static void AddEndpoints(WebApplication application)
        {
            application.MapGet("api/voc-map", GetVocabulary).WithOpenApi(operation => new(operation)
            {
                Summary = "Get vocabulary map",
            });
        }

        private static async Task<IResult> GetVocabulary(LingoDbContext dbcontext)
        {
            var userId = Guid.Empty;

            var words = await dbcontext.Words.ToArrayAsync();
            var progress = await dbcontext.UserWordProgresses.Where(p => p.User.Id == userId).ToDictionaryAsync(p=> p.WordID);            
            var result = words.Select(w => MapToWordProgress(w, progress.GetValueOrDefault(w.Id)));            
            return Results.Ok(result);
        }

        private static WordProgressDto MapToWordProgress(Word word, UserWordProgress? progress)
        {
            return new WordProgressDto()
            {
                Progress = progress?.MasteryLevel ?? 0,
                Wrd = word.Text,                
                X = word.XOnMap,
                Y = word.YOnMap
            };
        }
    }
}
