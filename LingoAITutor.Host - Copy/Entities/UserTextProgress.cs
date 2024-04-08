using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class UserTextProgress
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TextId { get; set; }
        public int SentenceNumber { get; set; }
    }
}
