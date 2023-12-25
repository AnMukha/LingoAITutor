using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LingoAITutor.Host.Endpoints
{    
    public static class TextEndpoint
    {
        public static void AddEndpoints(WebApplication application)
        {
            application.MapGet("api/texts", GetTexts).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Get texts list",
            });
        }

        private static async Task<IResult> GetTexts(LingoDbContext context, UserIdHepler userIdHelper)
        {
            var userId = userIdHelper.GetUserId();
            var texts = await context.Texts.Where(t => t.OwnerUserId == null || t.OwnerUserId == userIdHelper.GetUserId()).ToArrayAsync();
            var progresses = await context.UserTextProgresses.Where(p => p.UserId == userId).ToArrayAsync();
            var result = texts.Select(t => new TextTitleDto()
            {
                Id = t.Id,
                Title = t.Title,
                SentenceCount = t.SentenceCount,
                SentenceNumber = progresses.FirstOrDefault(p => p.TextId == t.Id)?.SentenceNumber?? 0
            }).OrderBy(t => t.Title).ToArray();
            return Results.Ok(result);
        }

    }
}
