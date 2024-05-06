using LingoAITutor.Host.Entities;

namespace LingoAITutor.Host.Services.LessonProgress.QuestionsLessonProgress
{
    public static class NextQuestionSelector
    {
        private static readonly Random _random = new Random();

        public static (int number, string? text) SelectNextQuestion(Lesson lesson)
        {
            var passedQuestions = ParsePassedQuestionNumbers(lesson.ProgressInfo);
            var questions = lesson.Scenario.Content!.Split(Environment.NewLine);
            if (!lesson.NextQuestionRandom)
            {
                if (passedQuestions.Count == 0)
                {
                    return (0, questions[0]);
                }
                var maxN = passedQuestions.Max();
                if (maxN + 1 >= questions.Length)
                {
                    return (0, questions[0]);
                }
                else
                {
                    return (maxN + 1, questions[maxN + 1]);
                }
            }
            else
            {
                var notPassed = new List<int>();
                for (int i = 0; i < questions.Length; i++)
                {
                    if (!passedQuestions.Contains(i))
                        notPassed.Add(i);
                }
                if (notPassed.Count == 0)
                    return (-1, null);
                var num = _random.Next(0, notPassed.Count);
                return (notPassed[num], questions[notPassed[num]]);
            }
        }

        public static HashSet<int> ParsePassedQuestionNumbers(string? progressInfo)
        {
            if (string.IsNullOrWhiteSpace(progressInfo))
                return new HashSet<int>();
            return new HashSet<int>(progressInfo.Split(',').Select(int.Parse));
        }

    }
}
