
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Services.Translation
{
    public class TranslationCache
    {
        LingoDbContext _dbContext;
        public TranslationCache(LingoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddTranslation(string sentence, string sourceLanguage, string targetLanguage, string translation)
        {
            _dbContext.Translations.Add(new Entities.Translation()
            {
                SourceLanguage = sourceLanguage,
                TargetLanguage = targetLanguage,
                SourceSentence = sentence,
                TranslatedSentence = translation,
                TranslationHash = CalculateHash(sentence, targetLanguage)
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task<string?> GetTranslation(string sentence, string sourceLanguage, string targetLanguage)
        {
            var hash = CalculateHash(sentence, targetLanguage);
            var translations = await _dbContext.Translations.Where(s => s.TranslationHash == hash).ToArrayAsync();
            var tr = translations.FirstOrDefault(t => t.SourceSentence == sentence && t.SourceLanguage == sourceLanguage && t.TargetLanguage == targetLanguage);
            if (tr == null) return null;
            return tr.TranslatedSentence;
        }

        private int CalculateHash(string sentence, string targetLanguage)
        {
            return (sentence + "%%%" + targetLanguage).GetHashCode();
        }
    }
}
