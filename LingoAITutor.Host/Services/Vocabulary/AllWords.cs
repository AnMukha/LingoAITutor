using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Services.Vocabulary
{
    public class AllWords
    {
        private List<Word>? _words = null;
        private Dictionary<string, Word>? _wordsByText = null;
        private IServiceScopeFactory _serviceScopeFactory;
        private Dictionary<int, int> _rangeSizes = new Dictionary<int, int>();

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
                _words = dbContext.Words.OrderBy(w => w.FrequencyRank).AsNoTracking().ToList();
                _wordsByText = _words.ToDictionary(w => w.Text);
            }
        }

        public Word? FindWordByText(string text)
        {
            if (_words == null) ReadWords();
            return _wordsByText!.TryGetValue(text, out Word? result) ? result : null;
        }

        public double GetCountInRange(int startPosition, int rangeSize)
        {
            if (_rangeSizes.TryGetValue(startPosition, out int size))
                return size;
            var result = GetWords().Where(w => w.FrequencyRank >= startPosition && w.FrequencyRank < startPosition + rangeSize).Count();
            _rangeSizes![startPosition] = result;
            return result;
        }
    }
}
