using System.ComponentModel.DataAnnotations;

namespace LingoAITutor.Host.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string? Email { get; set; }

        public virtual ICollection<UserWordProgress> UserWordProgresses { get; set; } = null!;
    }
}
