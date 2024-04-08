using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Entities.Enums;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace LingoAITutor.Host.Services.LessonProgress
{
    public class QuestionsLessonProgressor : ILessonProgressor
    {

        private readonly LingoDbContext _dbContext;
        private readonly OpenAIAPI _openAPI;

        public QuestionsLessonProgressor(LingoDbContext dbContext, OpenAIAPI openAPI)
        {
            _dbContext = dbContext;
            _openAPI = openAPI;
        }

        public async Task<Message?> ProgressLesson(Lesson lesson, ScenarioTemplate scenario)
        {            
            if (lesson.MessagesCount == 0)
            {
                return await StartLesson(lesson);
            }
            else
            {
                var messages = await _dbContext.Messages.Where(m => m.LessonId == lesson.LessonId && m.SectionNumber == lesson.SectionNumber)
                               .ToArrayAsync();
                messages = messages.OrderBy(m => m.Number).ToArray();
                if (messages.Last().MessageType != MessageType.UserMessage) return null;

                var chatMessages = ExtendMessages(messages);

                var gptResponseText = await GetGPTResponse(chatMessages);

                lesson.LastMessageNumber++;
                lesson.MessagesCount++;
                var gptMessage = new Message()
                {
                    MessageId = Guid.NewGuid(),
                    Content = gptResponseText,
                    MessageType = MessageType.GPTMessage,
                    Number = lesson.LastMessageNumber,
                    SectionNumber = lesson.SectionNumber
                };
                lesson.Messages.Add(gptMessage);
                _dbContext.Add(gptMessage);
                await _dbContext.SaveChangesAsync();
                return gptMessage;                
            }
        }

        private async Task<string> GetGPTResponse(IList<ChatMessage> messages)
        {
            var result = await _openAPI.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0,
                MaxTokens = 1000,
                Messages = messages
            });
            return result.Choices[0].Message.Content;
        }

        private List<ChatMessage> ExtendMessages(Message[] messages)
        {
            // первое сообщение - это вопрос, его не учитываем.
            // Второе - это ответ, его комбинируем с первым, получаем вопрос пользователя;
            // последующие - это обсуждение;
            // если они есть, просто переписываем их в историю.
            var result = new List<ChatMessage>();
            
            if (messages.Length < 2)
                return result;
            
            var questionAndAnswer = CombineAnswerAndResponce(messages[0].Content, messages[1].Content, ".Net developer");
            result.Add(new ChatMessage(ChatMessageRole.User, questionAndAnswer));

            for (var i = 2; i < messages.Length; i++)
            {
                var message = messages[i];
                result.Add(new ChatMessage(message.MessageType == MessageType.GPTMessage ? ChatMessageRole.Assistant : ChatMessageRole.User, message.Content));
            }
            return result;
        }

        private string CombineAnswerAndResponce(string? question, string? answer, string subject)
        {
            return "I am preparing for an interview for position " + subject + ". I want to prepare an answer to the following question:\n" +
            "\"" + question + "\"\n" +
            "Below is my answer to this question.Analyze my answer.Find inaccuracies in it and point them out. Estimate the completeness of the answer. If the answer is not entirely complete, describe what exactly I missed." +
            "My answer:\n" +
            answer;
        }

        private async Task<Message?> StartLesson(Lesson lesson)
        {
            var question = SelectNextQuestion(lesson);
            if (question.number == -1)
                return null;
            var message = new Message()
            {
                MessageId = Guid.NewGuid(),
                MessageType = Entities.Enums.MessageType.GPTMessage,
                Content = question.text,
                Number = 0,
                SectionNumber = 0
            };
            lesson.ProgressInfo = lesson.ProgressInfo + "," + question.number;
            lesson.Messages.Add(message);
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
            return message;
        }

        private readonly Random _random = new Random();

        private (int number, string? text) SelectNextQuestion(Lesson lesson)
        {
            var passedQuestions = ParsePassedQuestionNumbers(lesson);
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

        private HashSet<int> ParsePassedQuestionNumbers(Lesson lesson)
        {
            if (string.IsNullOrWhiteSpace(lesson.ProgressInfo))
                return new HashSet<int>();
            return new HashSet<int>(lesson.ProgressInfo.Split(',').Select(int.Parse));
        }

    }
}
