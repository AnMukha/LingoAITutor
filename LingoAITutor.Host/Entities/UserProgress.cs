using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class UserProgress
    {
        [Key]
        public Guid UserId { get; set; }
        public int ExerciseNumber { get; set; }
        public int EstimationNumber { get; set; }        
    }
}
