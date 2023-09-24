namespace LingoAITutor.Host.Dto
{
    public class WordTranslateFeedback
    {
        public string? Feedback { get; set; }
        public string? FixedPhrase { get; set; }
        public int[] WordMarks { get; set; } = Array.Empty<int>();
    }
}
