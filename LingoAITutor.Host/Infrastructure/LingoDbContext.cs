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
        public DbSet<UserProgress> UserProgresses { get; set; }
        public DbSet<UserTextProgress> UserTextProgresses { get; set; }
        public DbSet<RangeProgress> RangeProgresses { get; set; }
        public DbSet<Irregular> Irregulars { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Text> Texts { get; set; }
    }
}
