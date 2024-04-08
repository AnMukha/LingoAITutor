using LingoAITutor.Host.Entities;

namespace LingoAITutor.Host.Services.Interfaces
{
    public interface ILessonProgressor
    {
        Task<Message?> ProgressLesson(Lesson lesson, ScenarioTemplate scenario);
    }
}
