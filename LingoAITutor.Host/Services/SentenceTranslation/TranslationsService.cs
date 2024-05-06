using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Services.Translation
{
    public class TranslationsService
    {
        SentenceTranslator _translator;
        TranslationCache _cache;

        public TranslationsService(SentenceTranslator translator, TranslationCache cache)
        {
            _translator = translator;
            _cache = cache;
        }

        public async Task<string> Translate(string sentence, string sourceLanguage, string targetLanguage)
        {
            var cachedTranslation = await _cache.GetTranslation(sentence, sourceLanguage, targetLanguage);
            if (cachedTranslation != null)
                return cachedTranslation;
            var translation = await _translator.Translate(sentence, sourceLanguage, targetLanguage);
            await _cache.AddTranslation(sentence, sourceLanguage, targetLanguage, translation);
            return translation;
        }
    }
}
