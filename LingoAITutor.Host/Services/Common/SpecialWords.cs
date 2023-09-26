namespace LingoAITutor.Host.Services.Common
{
    // Words not needed in vocabulary training and not needed in 
    public static class SpecialWords
    {        
        public static readonly string[] Articles = new[] { "the", "a" };
        public static readonly string[] TrivialModals = new[] { "can", "could", "will", "would" };
        public static readonly string[] ServiceVerbs = new[] { "be", "am", "is", "are", "was", "were", "being", "been", "have", "had" };
        public static readonly string[] TrivialPrepositions = new[] { "as", "at", "by", "for", "in", "into", "of", "on", "out", "to" };

        /// <summary>
        /// Missing of replacing this words can not be fixed by studying vocabulary (it's about grammar)
        /// </summary>
        public static readonly HashSet<string> GrammarWords = new(Articles.Concat(TrivialModals).Concat(ServiceVerbs).Concat(TrivialPrepositions));
    }
}
