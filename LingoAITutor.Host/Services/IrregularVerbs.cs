using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Services
{
    public class IrregularVerbs
    {
        private List<Irregular>? _irregularWords = null;        
        
        private IServiceScopeFactory _serviceScopeFactory;
        

        public IrregularVerbs(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public List<Irregular> GetIrregulars()
        {
            if (_irregularWords == null) ReadWords();
            return _irregularWords!;
        }

        public string? FindFirstForm(string verb)
        {
            foreach(var v in GetIrregulars())
            {
                if (v.V1 == verb || v.V2 == verb || v.V3 == verb)
                    return v.V1;
            }
            return null;
        }

        private void ReadWords()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<LingoDbContext>();
                _irregularWords = dbContext.Irregulars.AsNoTracking().ToList();                
            }
        }
    }
}
