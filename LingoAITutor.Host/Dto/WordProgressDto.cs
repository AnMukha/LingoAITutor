﻿namespace LingoAITutor.Host.Dto
{
    public class WordProgressDto
    {        
        public string Wrd { get; set; } = null!;
        public int X { get; set; }
        public int Y { get; set; }        
        public int CorrectUses { get; set; }
        public int NonUses { get; set; }
        public bool Failed { get; set; }
    }
}
