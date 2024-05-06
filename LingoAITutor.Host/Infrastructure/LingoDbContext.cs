using LingoAITutor.Host.Entities;
using Microsoft.EntityFrameworkCore;
using System.Xml;

namespace LingoAITutor.Host.Infrastructure
{
    public class LingoDbContext: DbContext
    {
        public LingoDbContext(DbContextOptions<LingoDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Translation>()
                .HasIndex(e => e.TranslationHash);
        }

        public DbSet<Word> Words { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserWordProgress> UserWordProgresses { get; set; }
        public DbSet<UserProgress> UserProgresses { get; set; }                
        public DbSet<Irregular> Irregulars { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Text> Texts { get; set; }
        public DbSet<ScenarioTemplate> ScenarioTemplates { get; set; }
        public DbSet<Translation> Translations { get; set; }

    }
}
