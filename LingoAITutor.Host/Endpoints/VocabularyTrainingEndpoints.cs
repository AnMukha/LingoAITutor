using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Services;
using Microsoft.AspNetCore.Mvc;

namespace LingoAITutor.Host.Endpoints
{

    public static class VocabularyTrainingEndpoints
    {        
        public static void AddEndpoints(WebApplication application)
        {
            application.MapGet("api/voc-train-next", GetNextExercise).WithOpenApi(operation => new(operation)
            {
                Summary = "Get next excercise for vocabulary train",
            });
            application.MapPost("api/voc-train-submit", SubmitAnswer).WithOpenApi(operation => new(operation)
            {
                Summary = "Submit excercise answer for vocabulary train",
            });
        }

        private async static Task<IResult> GetNextExercise(TranslationExerciseGenerator generator)
        {
            return Results.Ok(await generator.GetNextExercise());
        }

        private async static Task<IResult> SubmitAnswer(TranslationExerciseAnaliser analiser, [FromBody] AnswerDto answer)
        {
            return Results.Ok(await analiser.AnalyseAnswer(answer));
        }        

    }
}

