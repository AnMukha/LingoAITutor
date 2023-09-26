using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class RangeProgress
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserProgressId { get; set; }
        public UserProgress UserProgress { get; set; } = null!;
        public double? Progress { get; set; }
        public int WordsCount { get; set; }
        public int StartPosition { get; set; }
    }
}
