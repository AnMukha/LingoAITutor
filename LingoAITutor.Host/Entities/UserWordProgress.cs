using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class UserWordProgress
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserID { get; set; }
        public Guid WordID { get; set; }
        public int MasteryLevel { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual Word Word { get; set; } = null!;
    }

}
