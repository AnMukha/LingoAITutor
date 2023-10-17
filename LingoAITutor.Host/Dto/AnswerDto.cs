using LingoAITutor.Host.Entities.Enums;

namespace LingoAITutor.Host.Dto
{
    public class AnswerDto
    {
        public string? OriginalPhrase { get; set; }
        public string? ExerciseText { get; set; }
        public string? AnswerText { get; set; }
        public string? Word { get; set; }
        public NextWordStrategy Strategy { get; set; }
    }
}
