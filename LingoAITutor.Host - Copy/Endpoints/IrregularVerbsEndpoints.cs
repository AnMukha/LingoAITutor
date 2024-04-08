using LingoAITutor.Host.Services;

namespace LingoAITutor.Host.Endpoints
{
    public class IrregularVerbsEndpoints
    {
        public static void AddEndpoints(WebApplication application)
        {
            application.MapGet("api/irregular-verb-practice-next", GetNextExercise).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Get next task on irregular verbs",
            });
            application.MapPost("api/irregular-verb-practice-submit", Submit).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Submit response on irregular verbs",
            });

        }

        private static async Task<IResult> GetNextExercise(IrregularVerbExerciseGenerator taskGenerator)
        {
            return Results.Ok(await taskGenerator.GetNextExercise());
        }

        private static async Task<IResult> Submit()
        {
            return Results.Ok();
        }

    }
}
