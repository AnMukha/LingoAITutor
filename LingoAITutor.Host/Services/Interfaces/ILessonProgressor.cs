using LingoAITutor.Host.Entities;

namespace LingoAITutor.Host.Services.Interfaces
{
    public interface ILessonProgressor
    {
        Task<Message?> ProgressLesson(Lesson lesson);
        Task<Message?> ToNextSection(Lesson lesson, ScenarioTemplate scenario);
    }
}
