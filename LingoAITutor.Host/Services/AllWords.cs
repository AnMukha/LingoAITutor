using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Services
{
    public class AllWords
    {        
        private List<Word>? _words = null;
        private Dictionary<string, Word>? _wordsByText = null;
        private IServiceScopeFactory _serviceScopeFactory;

        public AllWords(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public List<Word> GetWords()
        {
            if (_words == null) ReadWords();
            return _words!;
        }

        private void ReadWords()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<LingoDbContext>();
                _words = dbContext.Words.AsNoTracking().ToList();
                _wordsByText = _words.ToDictionary(w => w.Text);
            }
        }

        public Word? FindWordByText(string text)
        {
            if (_words == null) ReadWords();
            return _wordsByText!.TryGetValue(text, out Word? result) ? result : null;
        }
    }
}
