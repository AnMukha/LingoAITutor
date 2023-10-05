using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Services;

namespace LingoAITutor.Host.Utilities
{
    public class VocabluaryImport
    {
        private readonly LingoDbContext _dbContext;        

        public VocabluaryImport(LingoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Import(string pathToFile1, string pathToFile2, string pathToFile3)
        {
            var processedWords = new List<string>();

            var wordsFromFile1 = DistinctKeepingOrder(File.ReadAllLines(pathToFile1));
            processedWords.AddRange(wordsFromFile1);

            var wordsFromFile2 = DistinctKeepingOrder(File.ReadAllLines(pathToFile2));
            foreach (var word in wordsFromFile2)
            {
                if (!processedWords.Contains(word))
                {
                    int position = FindPosition(word, wordsFromFile2, processedWords);
                    processedWords.Insert(position, word);
                }
            }

            var wordsFromFile3 = DistinctKeepingOrder(File.ReadAllLines(pathToFile3));
            // wordsFromFile3 = ExcludeForms(wordsFromFile3);
            foreach (var word in wordsFromFile3)
            {
                if (!processedWords.Contains(word) && !processedWords.Any(w => TranslationExerciseAnaliser.IsSameWord(word, w)))
                {
                    processedWords.Add(word);
                }
            }

            var words = processedWords.Select((w, n) => new Word()
            {
                Id = Guid.NewGuid(),
                Text = w,
                FrequencyRank = n
            }).ToList();

            WordPositionOnMapCalc.CaculatePositionsOnTheMap(words);

            SaveToDataBase(words);
        }


        private int FindPosition(string word, List<string> wordsFromFile2, List<string> processedWords)
        {
            int wordIndex = wordsFromFile2.IndexOf(word);

            if (wordIndex == -1)
            {
                throw new ArgumentException($"The word '{word}' was not found in the file.");
            }

            var surroundingWords = new List<string>();
            int start = Math.Max(0, wordIndex - 5);
            int end = Math.Min(wordsFromFile2.Count - 1, wordIndex + 5);

            for (int i = start; i <= end; i++)
            {
                surroundingWords.Add(wordsFromFile2[i]);
            }

            // Find the positions of the surrounding words in the current result list
            var positionsInResultList = surroundingWords
                .Select(w => processedWords.IndexOf(w))
                .Where(index => index != -1)
                .ToList();

            if (!positionsInResultList.Any())
            {
                throw new InvalidOperationException("None of the surrounding words were found in the current result list.");
            }

            // Calculate the average position
            double averagePosition = positionsInResultList.Average();

            return (int)Math.Round(averagePosition);
        }

        private List<string> ExcludeForms(List<string> words)
        {
            var wordSet = new HashSet<string>(words);
            return words.Where(w => !IsPluralForm(w, words)).ToList();
        }

        private bool IsPluralForm(string word, List<string> words)
        {
            if (word.EndsWith("es") && words.Contains(word.Substring(0, word.Length - 2)))
            {
                return true;
            }
            if (word.EndsWith("s") && words.Contains(word.Substring(0, word.Length - 1)))
            {
                return true;
            }
            return false;            
        }

        private List<string> DistinctKeepingOrder(string[] words)
        {
            var seen = new HashSet<string>();
            var result = new List<string>();
            foreach (var str in words)
            {
                if (seen.Add(str)) result.Add(str);
            }
            return result;
        }


        private void SaveToDataBase(IEnumerable<Word> words)
        {            
            var allWords = _dbContext.Words.ToList();
            _dbContext.Words.RemoveRange(allWords);
            _dbContext.SaveChanges();

            foreach(var w in words)
            {
                _dbContext.Words.Add(w);
            }
            _dbContext.SaveChanges();
        }

    }
}
