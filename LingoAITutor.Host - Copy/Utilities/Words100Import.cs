using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Services;
using LingoAITutor.Host.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;

namespace LingoAITutor.Host.Utilities
{
    public class Words100Import
    {
        private LingoDbContext _context;
        private HashSet<string> _notTryToFindForms;

        public Words100Import(LingoDbContext context)
        {
            _context = context;
            _notTryToFindForms = new HashSet<string>(SpecialWords.AllPronouns.Concat(SpecialWords.Articles).Concat(SpecialWords.OthersGrammar)
                            .Concat(SpecialWords.PhraseComponents).Concat(SpecialWords.ServiceVerbs).Concat(SpecialWords.TrivialModals)
                            .Concat(SpecialWords.TrivialPrepositions).Concat(SpecialWords.NotNounOrVerb).Distinct());
        }

        public void UpdateWords(string fileName, string propNamesFileName, string notEsWordsFileName)
        {

            CopyWords();
            ImportWords(fileName, propNamesFileName, notEsWordsFileName);
            RestoreProgress();
        }

        private void RestoreProgress()
        {
            var allWords = _context.Words.ToArray();
            var toDelete = new HashSet<UserWordProgress>();
            foreach (var p in _context.UserWordProgresses.ToArray())
            {
                var word = allWords.FirstOrDefault(w => w.Text == p.WordText);
                if (word != null)
                    p.WordID = word.Id;
                else
                    toDelete.Add(p);
            }
            foreach (var p in toDelete)
                _context.UserWordProgresses.Remove(p);
            _context.SaveChanges();
        }

        class WordInfo
        {
            public string Word = string.Empty;
            public double freq;
            public int number;
        }

        private void ImportWords(string fileName, string propNamesFileName, string notEsWordsFileName)
        {
            var lines = File.ReadAllLines(fileName);
            var wds = lines.Select(ParseLine).ToArray();
            var filteredWrds = FilterNotVocabularyWords(wds, propNamesFileName);
            var notEsWords = new HashSet<string>(File.ReadAllLines(notEsWordsFileName));
            var words = filteredWrds.ToDictionary(w => w.Word);
            var passed = new HashSet<string>();
            var result = new List<WordInfo>();
            foreach (var w in words.Where(w => !notEsWords.Contains(w.Key)))
            {
                if (!passed.Contains(w.Key) && !_notTryToFindForms.Contains(w.Key))
                
                {
                    var forms = FindForms(w.Key, words);                    
                    // forms found, it is a root form   
                    if (forms.Length > 1 )
                    {
                        if (!(words[forms[0]].number < 3000 && words[forms[1]].number > 30000))
                        {
                            foreach (var f in forms)
                                passed.Add(f);
                            var mainForm = JoinForms(forms, words);
                            result.Add(mainForm);
                        }
                    }
                }
            }
            // the rest of the words that was not picked up with forms are root or single too
            var rest = words.Where(w => !passed.Contains(w.Key)).Select(w => w.Value).ToArray();
            result.AddRange(rest);
            
            var shortened = result.Where(w => w.number <= 30000).ToArray();
            var ordered = shortened.GroupBy(r => r.freq).OrderByDescending(g => g.Key).SelectMany(g => g.OrderBy(w => w.number)).ToArray();
            _context.Words.RemoveRange(_context.Words);
            _context.SaveChanges();
            var resultWords = new List<Word>();
            for(var n=0; n< ordered.Length; n++)
            {
                resultWords.Add(new Word()
                {
                    Id = Guid.NewGuid(),
                    FrequencyRank = n,
                    Text = ordered[n].Word
                });
            }
            WordPositionOnMapCalc.CaculatePositionsOnTheMap(resultWords);
            _context.Words.AddRange(resultWords);
            _context.SaveChanges();
        }

        private WordInfo[] FilterNotVocabularyWords(WordInfo[] words, string propNamesFileName)
        {
            var lettersRemoved = words.Where(w => w.Word.Length > 1 || w.Word == "i" || w.Word == "a").ToArray();
            var withoutVovelRemoved = lettersRemoved.Where(w => w.Word.Count(l => IsVovel(l)) > 0).ToArray();
            var withoutTwoLetters = withoutVovelRemoved.Where(w => w.Word.Length!=2 || SpecialWords.TwoLetterWords.Contains(w.Word)).ToArray();
            var properNames = File.ReadAllLines(propNamesFileName).ToHashSet();
            var withoutProper = withoutTwoLetters.Where(w => !properNames.Contains(w.Word)).ToArray();
            return withoutProper;
        }

        private WordInfo JoinForms(string[] forms, Dictionary<string, WordInfo> allWords)
        {
            var mainForm = forms.OrderBy(f => f.Length).First();
            var words = forms.Select(f => allWords[f]).ToArray();
            return new WordInfo()
            {
                Word = mainForm,
                number = words.Min(w => w.number),
                freq = words.Sum(w => w.freq)
            };
        }

        private string[] FindForms(string word, Dictionary<string, WordInfo> words)
        {
            var forms = new string[]
            {
                word,
                //MakeEdForm(word),
                //MakeIngForm(word),
                //MakeAlterIngForm(word),
                MakeEsForm(word),
                //MakeAlterEsForm(word)
            };
            return forms.Distinct().Where(w => w != string.Empty && words.ContainsKey(w)).ToArray();
        }

        private string MakeEsForm(string word)
        {
            if (word.Length == 1)
                return string.Empty;
            if (word.EndsWith("s") || word.EndsWith("x") || word.EndsWith("z") || word.EndsWith("ch") || word.EndsWith("sh"))
                return word + "es";
            if (word.EndsWith("th"))
                return word + "s";
            if (word[word.Length - 1] == 'y' && !IsVovel(word[word.Length - 2]))
                return word.Substring(0, word.Length - 1) + "ies";
            if (word[word.Length - 1] == 'y' && IsVovel(word[word.Length - 2]))
                return word + "s";
            if (word[word.Length - 1] == 'o')
                return word + "es";
            return word + "s";
        }

        private string MakeAlterEsForm(string word)
        {
            if (word[word.Length - 1] == 'o' && IsVovel(word[word.Length - 2]))
                return word + "s";
            return string.Empty;
        }

        private string MakeIngForm(string word)
        {
            if (word.Length == 1)
                return string.Empty;
            if (word[word.Length - 1] == 'e')
                return word.Substring(0, word.Length-1) + "ing";
            if (word[word.Length - 2] == 'i' && word[word.Length - 1] == 'e')
                return word.Substring(0, word.Length - 2) + "ying";
            if (word[word.Length - 2] == 'i' && word[word.Length - 1] == 'c')
                return word + "king";
            if (IsVovel(word[word.Length - 1]) && !IsVovel(word[word.Length - 2]))
                return word + word[word.Length - 1] + "ing";
            return word + "ing";
        }

        private string MakeAlterIngForm(string word)
        {
            // without doubling end letter
            if (word.Length>1 && IsVovel(word[word.Length - 1]) && !IsVovel(word[word.Length - 2]))
                return word + "ing";
            return string.Empty;
        }

        private string MakeEdForm(string word)
        {
            if (word.Length == 1)
                return string.Empty;
            if (word[word.Length - 1] == 'y' && !IsVovel(word[word.Length - 2]))
                return word.Substring(0, word.Length - 1) + "ied";
            if (word[word.Length - 1] == 'e')
                return word + "d";
            if (IsVovel(word[word.Length - 1]) && !IsVovel(word[word.Length - 2]) && word.Count(IsVovel) == 1)
                return word + word[word.Length - 1] + "ed";
            return word + "ed";
        }

        private bool IsVovel(char v)
        {
            return "weyuioa".Contains(v);
        }

        private WordInfo ParseLine(string line)
        {
            var values = line.Split(',');
            return new WordInfo()
            {
                Word = values[0],
                freq = double.Parse(values[1]), 
                number = int.Parse(values[2])
            };
        }

        private void CopyWords()
        {
            foreach(var p in _context.UserWordProgresses.Include(w => w.Word).ToArray())
            {
                if (p.Word != null)
                {
                    p.WordText = p.Word.Text;
                    p.WordID = null;
                }
            }
            _context.SaveChanges();
        }
    }
}
