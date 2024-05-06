using LingoAITutor.Host.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; }
        public string? Content { get; set; }
        public string? CorrectedContent { get; set; }
        public string? Corrections { get; set; }
        public MessageType MessageType { get; set; }
        public bool SectionInitMessage { get; set; }
        public int Number { get; set; }
        public int SectionNumber { get; set; }
        public Guid LessonId { get; set; }
        public Lesson Lesson { get; set; } = null!;
    }
}
