namespace LingoAITutor.Host.Services.Common
{
    // Words not needed in vocabulary training and not needed in 
    public static class SpecialWords
    {        
        public static readonly string[] Articles = new[] { "the", "a", "an" };
        public static readonly string[] TrivialModals = new[] { "can", "could", "will", "would" };
        public static readonly string[] ServiceVerbs = new[] { "be", "am", "is", "are", "was", "were", "being", "been", "have", "has", "had", "do", "did" };
        public static readonly string[] TrivialPrepositions = new[] { "as", "at", "by", "for", "from", "in", "into", "of", "off", "on", "out", "to", "about", "with"};
        public static readonly string[] OthersGrammar = new[] { "n't", "not", "no", "yes", "or", "and", "but"};
        public static readonly string[] PhraseComponents = new[] { "up", "down", "get", "got" };
        public static readonly string[] NotInterestingAnnoying = new[] { "if", "oh", "language" };

        /// <summary>
        /// Missing of replacing this words can not be fixed by studying vocabulary (it's about grammar)
        /// </summary>
        public static readonly HashSet<string> NotAnalizedWords  = new(Articles.Concat(TrivialModals).Concat(ServiceVerbs).Concat(TrivialPrepositions).Concat(OthersGrammar).Concat(PhraseComponents).Concat(NotInterestingAnnoying));

        public static readonly HashSet<string> ToRemoveFromVocabulary = new(Articles.
                                                                            Concat(TrivialModals).
                                                                            Concat(ServiceVerbs).
                                                                            Concat(TrivialPrepositions).
                                                                            Concat(OthersGrammar));
    }
}
