using LingoAITutor.Host.Entities;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Infrastructure
{
    public class LingoDbContext: DbContext
    {
        public LingoDbContext(DbContextOptions<LingoDbContext> options) : base(options)
        {
        }


        public DbSet<Word> Words { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserWordProgress> UserWordProgresses { get; set; }

    }
}
