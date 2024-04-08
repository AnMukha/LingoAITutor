using LingoAITutor.Host.Entities.Enums;

namespace LingoAITutor.Host.Dto
{
    public class WordTranslateExerciseDto
    {
        public string Word { get; set; } = null!;
        public string? NativePhrase { get; set; }
        public string? OriginalPhrase { get; set; }
        public int Number { get; set; }
        public NextWordStrategy Strategy { get; set; }
    }
}
