using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LingoAITutor.Host.Utilities
{
    public class IrregularImport
    {
        LingoDbContext _dbContext;

        public IrregularImport(LingoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Import(string fileName)
        {            
            _dbContext.Irregulars.RemoveRange(_dbContext.Irregulars.ToList());
            var lines = File.ReadAllLines(fileName);
            foreach(var l in lines)
            {                
                var newIrrWord = new Irregular() { Id = Guid.NewGuid() };
                var formsLine = l.TrimEnd().TrimStart();
                if (l[0] == '*')
                {
                    newIrrWord.Optional = true;
                    formsLine = l.Substring(1);
                }                
                var words = formsLine.Split(' ');
                newIrrWord.V1 = words[0];
                newIrrWord.V2 = words[1];
                newIrrWord.V3 = words[2];
                _dbContext.Irregulars.Add(newIrrWord);
            }
            _dbContext.SaveChanges();
        }
    }
}
