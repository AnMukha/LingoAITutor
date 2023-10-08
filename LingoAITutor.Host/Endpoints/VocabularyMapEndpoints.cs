using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Services;
using System.Security.Claims;

namespace LingoAITutor.Host.Endpoints
{    
    public static class VocabularyMapEndpoints
    {
        public static void AddEndpoints(WebApplication application)
        {
            application.MapGet("api/voc-map", GetVocabulary).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Get vocabulary map",
            });
            application.MapGet("api/voc-size", GetVocabularySize).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Get vocabulary size",
            });
        }

        private static async Task<IResult> GetVocabulary(VocabularyMapGenerator map, UserIdHepler userIdHelper)
        {
            return Results.Ok(await map.GetMap(userIdHelper.GetUserId()));
        }

        private static async Task<IResult> GetVocabularySize(UserIdHepler uId, VocabularySizeCalculation vocCalc)
        {
            return Results.Ok(await vocCalc.CalculateVocabularySize(uId.GetUserId()));
        }
    }
}
