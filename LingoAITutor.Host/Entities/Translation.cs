using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class Translation
    {
        [Key]
        public int Id { get; set; }        
        public int TranslationHash { get; set; }
        public string? SourceSentence { get; set; }
        public string? TranslatedSentence { get; set; }
        public string? SourceLanguage { get; set; }
        public string? TargetLanguage { get; set; }
    }
}
