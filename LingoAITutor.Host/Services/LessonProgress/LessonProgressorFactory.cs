using LingoAITutor.Host.Entities.Enums;
using LingoAITutor.Host.Services.Interfaces;
using LingoAITutor.Host.Services.LessonProgress.QuestionsLessonProgress;

namespace LingoAITutor.Host.Services.LessonProgress
{
    public class LessonProgressorFactory
    {        
        IServiceProvider _serviceProvider;
        public LessonProgressorFactory(IServiceProvider serviceProvider) 
        {            
            _serviceProvider = serviceProvider;
        }

        public ILessonProgressor CreateProgressor(ScenarioType scenarioType)
        {
            switch (scenarioType)
            {
                case ScenarioType.Questions:
                    return _serviceProvider.GetService<QuestionsLessonProgressor>()!;
                case ScenarioType.Translation:
                    return _serviceProvider.GetService<TranslationLessonProgressor>()!;
                case ScenarioType.FreeChat:
                    return _serviceProvider.GetService<FreeChatProgressor>()!;
                default:
                    throw new Exception("Progress scenario not implemented");
            }
        }

    }
}
