using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Services.Common;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace LingoAITutor.Host.Utilities
{
    public class DBSeeder
    {
        IServiceScope _scope;
        LingoDbContext _context;

        public DBSeeder(IServiceScope scope)
        {
            _scope = scope;
            _context = _scope.ServiceProvider.GetRequiredService<LingoDbContext>();
        }

        public void Seed()
        {
            if (!TryMigrate()) return;

            var path = GetPath();

            SeedWords(path);

            SeedIrregular(path);

            //_context.Words.Where(w => SpecialWords.NotAnalizedWords.Contains(w.Text)).ExecuteDelete();

            SeedUsers(path);

            SeedTexts(path);

            _context.SaveChanges();
        }

        private void SeedTexts(string path)
        {
            string[] fileEntries = Directory.GetFiles(Path.Combine(path, "texts"));
            foreach (string fileName in fileEntries)
                SeedText(fileName);
        }

        private void SeedText(string fileName)
        {
            var text = File.ReadAllLines(fileName);
            var title = text.First();
            if (_context.Texts.Any(t => t.Title == title)) return;
            var newText = new Text();
            newText.Id = Guid.NewGuid();
            newText.Title = title;
            newText.SentenceCount = text.Length-1;
            _context.Texts.Add(newText);
            for(var i = 1; i < text.Length; i++)
            {
                var sentence = new TextSentence()
                {
                    Id = Guid.NewGuid(),
                    ParentTextId = newText.Id,
                    Content = text[i],
                    Number = i
                };
            }
        }

        private void SeedUsers(string path)
        {
            if (!_context.Users.Any(u => u.UserName == "ilka"))
            {
                _context.Users.Add(new User()
                {
                    Id = Guid.NewGuid(),
                    UserName = "ilka",
                    Email = "ilka@test.com",
                    PasswordHash = "AQAAAAIAAYagAAAAEK0CjBl+Cyd8TCixgR0noN4PRLxq2u7lLZsDUJVGRE68NO9HerDnYY12X4BrI6mxQA=="
                });
            }
            if (!_context.Users.Any(u => u.UserName == "mukha"))
            {
                _context.Users.Add(new User()
                {
                    Id = Guid.NewGuid(),
                    UserName = "mukha",
                    Email = "mukha@test.com",
                    PasswordHash = "AQAAAAIAAYagAAAAEK0CjBl+Cyd8TCixgR0noN4PRLxq2u7lLZsDUJVGRE68NO9HerDnYY12X4BrI6mxQA=="
                });
            }
        }

        private void SeedIrregular(string path)
        {
            if (!_context.Irregulars.Any())
            {
                Log.Information("Read irregular from file ", Path.Combine(path, "irregular.txt").ToString());
                var irregularImport = _scope.ServiceProvider.GetRequiredService<IrregularImport>();
                irregularImport.Import(Path.Combine(path, "irregular.txt"));
            }
        }

        private void SeedWords(string path)
        {
            if (_context.Words.Count() < 100000)
            {
                var w100Import = _scope.ServiceProvider.GetRequiredService<Words100Import>();
                w100Import.UpdateWords(Path.Combine(path, "words.txt"), Path.Combine(path, "propernames.txt"), Path.Combine(path, "not_es_words.txt"));
            }

            /*if (!_context.Words.Any())
            {
                var vocabluaryImport = _scope.ServiceProvider.GetRequiredService<VocabluaryImport>();
                vocabluaryImport.Import(Path.Combine(path, "cambridge.txt"), Path.Combine(path, "COCA 5000.txt"), Path.Combine(path, "10000.txt"));
            }*/
        }

        private string GetPath()
        {
#if DEBUG
            var env = _scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            var path = Path.Combine(env.ContentRootPath, "Txt");
#else
    var path = "txt/";
#endif
            return path;
        }

        private bool TryMigrate()
        {
            try
            {
                _context.Database.Migrate();
            }
            catch
            {
                Log.Logger.Error("Error on database migration attempt.");
                return false;
            }
            return true;
        }
    }
}
