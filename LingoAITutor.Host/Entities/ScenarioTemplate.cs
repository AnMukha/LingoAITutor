using LingoAITutor.Host.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class ScenarioTemplate
    {
        [Key]
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Preface { get; set; }
        public ScenarioType ScenarioType { get; set; }
        public string? Content { get; set; }
        public AIMode AIModeInChat { get; set; }
        public bool NextQuestionRandom { get; set; }
    }
}
