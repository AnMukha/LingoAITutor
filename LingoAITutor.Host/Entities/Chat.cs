using LingoAITutor.Host.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LingoAITutor.Host.Entities
{
    public class Chat
    {
        [Key]
        public Guid ChatId { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public ChatType ChatType { get; set; }
        public string? Title { get; set; }
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public int LastMessageNumber { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }
    }
}
