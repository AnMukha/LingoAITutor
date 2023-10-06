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
            application.MapGet("api/voc-size", GetVocabularySize).WithOpenApi(operation => new(operation)
            {
                Summary = "Get vocabulary size",
            });

        }

        private static async Task<IResult> GetVocabulary(VocabularyMapGenerator map)
        {
            return Results.Ok(await map.GetMap());
        }

        private static async Task<IResult> GetVocabularySize(VocabularySizeCalculation vocCalc)
        {
            return Results.Ok(await vocCalc.CalculateVocabularySize());
        }
    }
}
