using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class Irregular
    {
        [Key]
        public Guid Id { get; set; }
        public string V1 { get; set; } = string.Empty;
        public string V2 { get; set; } = string.Empty;
        public string V3 { get; set; } = string.Empty;
        public bool Optional { get; set; }
    }
}
