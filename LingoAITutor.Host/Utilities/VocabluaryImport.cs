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
                if (!processedWords.Contains(word) && !processedWords.Any(w => IsSameWord(word, w)))
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

        public static bool IsSameWord(string w1, string w2, IrregularVerbs? iv = null)
        {
            var wl1 = w1.ToLower();
            var wl2 = w2.ToLower();
            if (wl1 == wl2) return true;
            var n1 = NormalizeWord(wl1, iv);
            var n2 = NormalizeWord(wl2, iv);
            return (n1 == n2 || n1 + "e" == n2 || n1 == n2 + "e");
        }

        private static string NormalizeWord(string w, IrregularVerbs? iv)
        {
            if (iv is not null)
            {
                var v1 = iv.FindFirstForm(w);
                if (v1 != null) return v1;
            }
            if (w.EndsWith("ies")) return w[..^3] + "y";
            if (w.EndsWith("es")) return w[..^2];
            if (w.EndsWith("s") && w.Length > 1 && w[w.Length - 2] != 's' && w[w.Length - 2] != 'h' && w[w.Length - 2] != 'x') return w[..^1];
            if (w.EndsWith("ed")) return w[..^2];

            if (w.EndsWith("ing"))
            {
                if (w.Length > 4 && w[w.Length - 4] == w[w.Length - 5])
                    return w[..^4];
                return w[..^3];
            }
            return w;
        }


    }
}
