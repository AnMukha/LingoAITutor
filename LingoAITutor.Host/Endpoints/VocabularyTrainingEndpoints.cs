using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Services;
using LingoAITutor.Host.Utilities;
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

#if DEBUG
            application.MapPost("api/process-names", ProcessNames).WithOpenApi(operation => new(operation)
            {
                Summary = "Process names",
            });
#endif
        }

        private async static Task<IResult> GetNextExercise(TranslationExerciseGenerator generator)
        {
            return Results.Ok(await generator.GetNextExercise());
        }

        private async static Task<IResult> SubmitAnswer(TranslationExerciseAnaliser analiser, [FromBody] AnswerDto answer)
        {
            return Results.Ok(await analiser.AnalyseAnswer(answer));
        }

        private async static Task<IResult> ProcessNames(NamesExcluding ne)
        {
            return Results.Ok(await ne.ProcessNamesFiles());
        }

    }
}

