using LingoAITutor.Host.Services;

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

        private static async Task<IResult> GetVocabulary(VocabularyMapGenerator map)
        {
            return Results.Ok(await map.GetMap());
        }
    }
}
