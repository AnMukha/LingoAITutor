using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class TextSentence
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ParentTextId { get; set; }
        public Text ParentText { get; set; } = null!;
        public string? Content { get; set; }
        public int Number { get; set; }
    }
}
