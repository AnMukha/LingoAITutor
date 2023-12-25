using Microsoft.AspNetCore.Http.HttpResults;

namespace LingoAITutor.Host.Services.Common
{
    // Words not needed in vocabulary training and not needed in 
    public static class SpecialWords
    {        
        public static readonly string[] Articles = new[] { "the", "a", "an" };
        public static readonly string[] TrivialModals = new[] { "can", "could", "will", "would" };
        public static readonly string[] ServiceVerbs = new[] { "be", "am", "is", "are", "was", "were", "being", "been", "have", "has", "had", "do", "did" };
        public static readonly string[] TrivialPrepositions = new[] { "as", "at", "by", "for", "from", "in", "into", "of", "off", "on", "out", "to", "about", "with"};
        public static readonly string[] OthersGrammar = new[] { "n't", "not", "no", "yes", "or", "and", "but", "if", "so"};
        public static readonly string[] PhraseComponents = new[] { "up", "down", "get", "got" };
        public static readonly string[] NotInterestingAnnoying = new[] { "oh", "language" };

        public static readonly string[] AllPronouns = new[] { "all", "another","any","anybody","anyone","anything","as","aught","both","each",
                                                               "other","either","enough","everybody","everyone","everything","few","he","her","hers","herself","him","himself","his","i","idem","it","its","itself","many",
                                                               "me","mine","most","my","myself","naught","neither","no one","nobody","none","nothing","nought","one","another","other","others","ought","our","ours","ourself",
                                                               "ourselves","several","she","some","somebody","someone","something","somewhat","such","suchlike","that","thee","their","theirs","theirself","theirselves","them",
                                                               "themself","themselves","there","these","they","thine","this","those","thou","thy","thyself","us","we","what","whatever","whatnot","whatsoever","whence","where","whereby",
                                                               "wherefrom","wherein","whereinto","whereof","whereon","wherever","wheresoever","whereto","whereunto","wherewith","wherewithal","whether","which","whichever",
                                                               "whichsoever","who","whoever","whom","whomever","whomso","whomsoever","whose","whosever","whosesoever","whoso","whosoever","ye","yon","yonder","you","your","yours","yourself","yourselves" }.Distinct().ToArray();

        public static readonly string[] NotNounOrVerb = new[] { "more" };

        public static readonly string[] TwoLetterWords = new[] {
                                "of","to","in","it","is","on","he","be","as","by","at","or","we","an","do","if","so","no","up","my","me","go","am","us","eh",
                                 "ok","re","ad","el","et","pa","hi","ex","id","os","fo","ay","ox","bi"};


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

