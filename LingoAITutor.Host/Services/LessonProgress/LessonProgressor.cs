using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;

namespace LingoAITutor.Host.Services.LessonProgress
{
    public class LessonProgressor
    {
        LingoDbContext _dbContext;
        IServiceProvider _serviceProvider;
        public LessonProgressor(LingoDbContext dbContext, IServiceProvider serviceProvider) 
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
        }

        public async Task<Message?> ProgressLesson(Lesson lesson, ScenarioTemplate scenario)
        {
            if (lesson == null) return null;
            if (scenario.ScenarioType == Entities.Enums.ScenarioType.Translation)
                return await new TranslationLessonProgressor().ProgressLesson(lesson, scenario);
            else if (lesson.Scenario.ScenarioType == Entities.Enums.ScenarioType.Questions)
                return await new QuestionsLessonProgressor(_dbContext, _serviceProvider.GetService<OpenAIAPI>()!).ProgressLesson(lesson, scenario);
            else
                return await new FreeChatProgressor(_dbContext, _serviceProvider.GetService<OpenAIAPI>()!).ProgressLesson(lesson, scenario);

        }
    }
}
