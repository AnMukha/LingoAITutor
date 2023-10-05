using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LingoAITutor.Host.Endpoints
{

    public static class VocabularyTrainingEndpoints
    {        
        public static void AddEndpoints(WebApplication application)
        {
            application.MapGet("api/voc-train-next",  GetNextExercise).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Get next excercise for vocabulary train",
            });
            application.MapPost("api/voc-train-submit", SubmitAnswer).RequireAuthorization().WithOpenApi(operation => new(operation)
            {
                Summary = "Submit excercise answer for vocabulary train",
            });
        }

        private async static Task<IResult> GetNextExercise(ClaimsPrincipal cl, TranslationExerciseGenerator generator)
        {
            var user = cl.FindFirst(claim => claim.Type == "id");
            var userId = Guid.Parse(user.Properties["id"]);
            return Results.Ok(await generator.GetNextExercise());
        }

        private async static Task<IResult> SubmitAnswer(TranslationExerciseAnaliser analiser, [FromBody] AnswerDto answer)
        {
            return Results.Ok(await analiser.AnalyseAnswer(answer));
        }        

    }
}

