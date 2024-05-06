using LingoAITutor.Host.Infrastructure;

namespace LingoAITutor.Host.Repositories
{
    public class MessagesRep
    {
        private readonly LingoDbContext _dbContext;
        
        public MessagesRep(LingoDbContext dbContext)
        {
            _dbContext = dbContext;
        }


    }
}
