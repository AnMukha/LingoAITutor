using LingoAITutor.Host.Entities.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace LingoAITutor.Host.Dto
{
    public class ChatDto
    {
        public Guid? ChatId { get; set; }
        public ChatType ChatType { get; set; }
        public string? Title { get; set; }        
    }
}
