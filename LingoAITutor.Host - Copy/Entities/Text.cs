using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class Text
    {
        [Key]
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public Guid? OwnerUserId { get; set; }
        public int SentenceCount { get; set; }
        public virtual ICollection<TextSentence> Sentences { get; set; } = null!;
    }
}
