﻿namespace LingoAITutor.Host.Dto
{
    public class WordTranslateExerciseDto
    {
        public string Word { get; set; } = null!;
        public string? NativePhrase { get; set; }
        public string? OriginalPhrase { get; set; }
    }
}