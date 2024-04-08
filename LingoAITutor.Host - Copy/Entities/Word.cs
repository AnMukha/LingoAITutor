using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class Word
    {
        [Key]
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public float FrequencyRank { get; set; }
        public virtual ICollection<UserWordProgress> UserWordProgresses { get; set; } = null!;
        public int XOnMap { get; set; }
        public int YOnMap { get; set; }
    }    
}
