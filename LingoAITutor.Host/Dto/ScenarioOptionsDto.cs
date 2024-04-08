using LingoAITutor.Host.Entities.Enums;

namespace LingoAITutor.Host.Dto
{
    public class ScenarioOptionsDto
    {
        public Guid ScenarioId { get; set; }
        public AIMode? AIMode { get; set;}
    }
}
