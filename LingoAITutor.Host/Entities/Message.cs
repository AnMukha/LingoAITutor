using LingoAITutor.Host.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; }
        public string? Content { get; set; }
        public MessageType MessageType { get; set; }
        public int Number { get; set; }
    }
}
