using Microsoft.AspNetCore.Identity;

namespace LingoAITutor.Host.Entities
{
    public class User: IdentityUser<Guid>
    {
        public virtual ICollection<UserWordProgress> UserWordProgresses { get; set; } = null!;
        public virtual ICollection<Lesson> Lessons { get; set; } = null!;
    }
}
