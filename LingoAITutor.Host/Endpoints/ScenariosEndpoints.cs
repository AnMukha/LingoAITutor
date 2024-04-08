using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Endpoints
{
    public static class ScenariosEndpoints
    {
        public static void AddEndpoints(WebApplication application)
        {
            application.MapGet("api/scenarios", GetScenarios).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Get all scenarios",
            });
        }

        private static async Task<IResult> GetScenarios(LingoDbContext dbContext)
        {
            return Results.Ok(await dbContext.ScenarioTemplates.Select(s => Map(s)).ToArrayAsync());
        }

        private static ScenarioTemplateDto Map(ScenarioTemplate scenario)
        {
            return new ScenarioTemplateDto()
            {
                 Id = scenario.Id,
                 AIModeInChat = scenario.AIModeInChat,
                 Content = scenario.Content,
                 Description = scenario.Description,
                 ScenarioType = scenario.ScenarioType, 
                 Title = scenario.Title
            };
        }
    }
}
