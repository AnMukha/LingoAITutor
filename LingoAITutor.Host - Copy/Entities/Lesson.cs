using LingoAITutor.Host.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LingoAITutor.Host.Entities
{
    public class Lesson
    {
        [Key]
        public Guid LessonId { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public LessonType LessonType { get; set; }
        public string? Title { get; set; }
        public DateTime Created { get; set; }
        public string? Scenario { get; set; }
        public int MessagesCount { get; set; }
        public int LowQualityCount { get; set; }
        public int RevisedCount { get; set; }
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public int LastMessageNumber { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }

        public Guid ScenarioId { get; set; }
        public string? CustomContent { get; set; }
        public AIMode AIModeInChat { get; set; }

    }
}
