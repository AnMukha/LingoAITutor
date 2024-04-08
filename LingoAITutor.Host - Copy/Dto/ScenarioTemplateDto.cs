using LingoAITutor.Host.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class ScenarioTemplateDto
    {        
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public ScenarioType ScenarioType { get; set; }
        public string? Content { get; set; }
        public AIMode AIModeInChat { get; set; }        
    }
}
