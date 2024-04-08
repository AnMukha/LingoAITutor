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
        public string? Title { get; set; }
        public DateTime Created { get; set; }        
        public int LowQualityCount { get; set; }
        public int RevisedCount { get; set; }
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public int LastMessageNumber { get; set; }
        public int MessagesCount { get; set; }
        public int SectionNumber { get; set; }
        public ScenarioTemplate Scenario { get; set; } = null!;
        public Guid ScenarioId { get; set; }
        public string? CustomContent { get; set; }
        public AIMode AIModeInChat { get; set; }
        public string? ProgressInfo { get; set; }
        public bool NextQuestionRandom { get; set; }        
    }
}
