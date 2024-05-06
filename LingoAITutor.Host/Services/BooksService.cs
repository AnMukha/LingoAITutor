

namespace LingoAITutor.Host.Services
{
    public class BooksService
    {
        class CachedBook
        {
            public string[] sentences = null!;
            public string? fileName;
        }

        private Dictionary<string, CachedBook> books = new Dictionary<string, CachedBook> ();

        public int GetSentenceCount(string bookFileName)
        {
            var book = EnsureInCache(bookFileName);
            return book.sentences.Length;
        }

        private CachedBook EnsureInCache(string bookFileName)
        {
            if (books.ContainsKey(bookFileName))
                return books[bookFileName];
            else
            {
                return UploadBookToCache(bookFileName);
            }
        }

        public string GetSentence(int sentenceNumber, string bookFileName)
        {
            var book = EnsureInCache(bookFileName);
            return book.sentences[sentenceNumber];
        }

        private CachedBook UploadBookToCache(string bookFileName)
        {
            var text = File.ReadAllText(bookFileName);
            var resultText = SplitToSentences(text);
            var book = new CachedBook();
            book.fileName = bookFileName;
            book.sentences = resultText.ToArray();
            books[bookFileName] = book;
            return book;
        }

        private string[] SplitToSentences(string text)
        {            
            var preparedText = text.ReplaceLineEndings(" ");
            var startSent = 0;
            var resultText = new List<string>();
            for (var i = 0; i < preparedText.Length; i++)
            {
                if ((preparedText[i] == '.' || preparedText[i] == '!' || preparedText[i] == '?') && EndOfSentence(preparedText, i))
                {
                    var sent = preparedText.Substring(startSent, i - startSent);
                    startSent = i + 1;
                    if (sent.Any(char.IsLetter))
                    {
                        sent = sent.Replace("   ", " ");
                        sent = sent.Replace("  ", " ");
                        if (sent.Length > 2)
                            resultText.Add(sent);
                    }
                }
            }
            return resultText.ToArray();
        }

        private static bool EndOfSentence(string text, int pos)
        {
            for(var i = pos; i< text.Length; i++)
            {
                if (char.IsLetter(text[i]))
                {
                    return char.IsUpper(text[i]);
                }
            }
            return true;
        }
    }
}
