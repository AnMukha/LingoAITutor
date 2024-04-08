using System.Text;

namespace LingoAITutor.Host.Services
{
    public static class CorrectionsComposer
    {
        class WordInfo
        {
            public string text = null!;
            public string postfix = "";
            public int? matchedTo;
            public double position;
        }

        public static string ComposeCorrections(string text, string correctedText)
        {
            var original = ToWordArray(text);
            var corrected = ToWordArray(correctedText);
            CalculatePositions(original);
            CalculatePositions(corrected);
            var orderedByDist = original.Select((w, n) => (num: n, closestInfo: FindClosestMatched(w, corrected)))
                                .Where(wd => wd.closestInfo.closestNum != null).OrderBy(wd => wd.closestInfo.distance).ToArray();
            foreach (var wd in orderedByDist)
            {
                if (!CrossOthers(wd.num, wd.closestInfo.closestNum!.Value, original))
                    original[wd.num].matchedTo = wd.closestInfo.closestNum;
            }
            return ComposeCorrectionsString(original, corrected);
        }

        private static string ComposeCorrectionsString(List<WordInfo> original, List<WordInfo> corrected)
        {
            var result = new StringBuilder();
            int lastCorrectedAdded = -1;
            foreach(var w in original)
            {
                if (w.matchedTo == null)
                    result.Append("%er%" + w.text + "%/%" + w.postfix);
                else
                {
                    if (w.matchedTo.Value - lastCorrectedAdded -1 > 0)
                        result.Append("%add%" + string.Join("", corrected.GetRange(lastCorrectedAdded+1, w.matchedTo.Value - lastCorrectedAdded -1).Select(w => w.text+ w.postfix)) + "%/%");
                    result.Append(w.text + w.postfix);
                    lastCorrectedAdded = w.matchedTo.Value;
                }
                if (false && w.postfix.Contains('.'))
                {
                    if (result.ToString().LastIndexOf("%eo") < result.ToString().LastIndexOf("%/%"))
                        result.Append("%eoes%");
                    if (result.ToString().LastIndexOf("%eo") < result.ToString().LastIndexOf("%/%"))
                        result.Append("%eos%");
                }
            }
            if (corrected.Count() - lastCorrectedAdded - 1 > 0)
                result.Append("%add%" + string.Join("", corrected.GetRange(lastCorrectedAdded + 1, corrected.Count() - lastCorrectedAdded - 1).Select(w => w.text + w.postfix)) + "%/%");
            //if (result.ToString().LastIndexOf("%et%") < result.ToString().LastIndexOf("%/%"))
                //result.Append("%et%");

            return result.ToString();
        }

        private static (double distance, int? closestNum) FindClosestMatched(WordInfo wi, List<WordInfo> corrected)
        {
            var matched = Enumerable.Range(0, corrected.Count()).Where(n => corrected[n].text == wi.text).ToArray();
            if (matched.Length == 0)
                return (0, null);
            var closest = matched.OrderBy(n => Math.Abs(corrected[n].position - wi.position)).First();
            return (Math.Abs(corrected[closest].position - wi.position), closest);
        }

        private static bool CrossOthers(int numInOriginal, int closestNum, List<WordInfo> original)
        {
            for(var i = 0; i < original.Count(); i++)
            {
                if (i != numInOriginal && original[i].matchedTo != null &&
                    ((i > numInOriginal && (original[i].matchedTo!.Value < closestNum)) ||
                     (i < numInOriginal && (original[i].matchedTo!.Value > closestNum))))
                    return true;
            }
            return false;
        }

        private static void CalculatePositions(List<WordInfo> words)
        {            
            double sumLength = words.Select(s => s.text.Length + s.postfix.Length).Sum();
            int currentWordPos = 0;
            for(var i = 0; i< words.Count(); i++)
            {
                words[i].position = (currentWordPos + words[i].text.Length / 2.0) * 100 / sumLength;
                currentWordPos += words[i].text.Length + words[i].postfix.Length;
            }
        }

        private static List<WordInfo> ToWordArray(string text)
        {
            var words = new List<WordInfo>();            
            int wordStart = -1;
            int delimiterStart = -1;
            // найти первую букву, выделить слово, если это не конец, выделить промежуток до следующего слова
            for(var i = 0; i< text.Length; i++)
            {
                if (char.IsLetter(text[i]))
                {
                    if (wordStart == -1)                    
                        wordStart = i;
                    if (delimiterStart != -1 && words.Any())
                    {
                        words.Last().postfix = text.Substring(delimiterStart, i - delimiterStart);
                        delimiterStart = -1;
                    }
                }
                else
                {
                    if (delimiterStart == -1)
                        delimiterStart = i;
                    if (wordStart != -1)
                    {
                        words.Add(new WordInfo() { text = text.Substring(wordStart, i - wordStart) });
                        wordStart = -1;
                    }
                }
            }
            if (delimiterStart != -1 && words.Any())            
                words.Last().postfix = text.Substring(delimiterStart, text.Length - delimiterStart);
            if (wordStart != -1)            
                words.Add(new WordInfo() { text = text.Substring(wordStart, text.Length - wordStart) });
            return words;
        }
    }
}
