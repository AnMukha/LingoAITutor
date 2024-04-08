using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LingoAITutor.Host.Entities
{
    public class User: IdentityUser<Guid>
    {
        public virtual ICollection<UserWordProgress> UserWordProgresses { get; set; } = null!;
        public virtual ICollection<Lesson> Lessons { get; set; } = null!;
    }
}
