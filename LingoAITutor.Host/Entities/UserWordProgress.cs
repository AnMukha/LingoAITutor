using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class UserWordProgress
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserID { get; set; }
        public Guid WordID { get; set; }
        public int CorrectUses { get; set; }
        public int ReplacedBySynonyms { get; set; }
        public int NonUses { get; set; }
        public bool FailedToUseFlag { get; set; }
        public string? FailedToUseSencence { get; set; }
        public DateTime? WorkOutStart { get; set; }
        public int? EstimationExerciseNumber { get; set; }
        public bool? EstimationExerciseResult { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual Word Word { get; set; } = null!;
    }

}
