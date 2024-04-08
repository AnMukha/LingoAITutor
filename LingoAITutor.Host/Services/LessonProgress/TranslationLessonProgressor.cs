using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Services.Interfaces;

namespace LingoAITutor.Host.Services.LessonProgress
{
    public class TranslationLessonProgressor : ILessonProgressor
    {
        public Task<Message?> ProgressLesson(Lesson lesson, ScenarioTemplate scenario)
        {
            throw new NotImplementedException();
        }
    }
}
