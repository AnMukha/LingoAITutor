using LingoAITutor.Host.Entities.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace LingoAITutor.Host.Dto
{
    public class LessonDto
    {
        public Guid? LessonId { get; set; }        
        public string? Title { get; set; }        
        public DateTime? Created { get; set; }
        public string? Scenario { get; set; }
        public int MessagesCount { get; set; }
        public int LowQualityCount { get; set; }
        public int RevisedCount { get; set; }
        public string? Preface { get; set; }
        public ScenarioType ScenarioType { get; set; }
    }

}
