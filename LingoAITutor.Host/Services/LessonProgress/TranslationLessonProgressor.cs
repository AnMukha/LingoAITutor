using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Services.Interfaces;
using LingoAITutor.Host.Services.Translation;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;

namespace LingoAITutor.Host.Services.LessonProgress
{
    public class TranslationLessonProgressor : ILessonProgressor
    {

        private readonly LingoDbContext _dbContext;
        private readonly OpenAIAPI _openAPI;
        private readonly BooksService _booksService;
        public readonly SentenceTranslator _sentenceTranslator;

        public TranslationLessonProgressor(LingoDbContext dbContext, OpenAIAPI openAPI, BooksService booksService, SentenceTranslator sentenceTranslator)
        {
            _dbContext = dbContext;
            _openAPI = openAPI;
            _booksService = booksService;
            _sentenceTranslator = sentenceTranslator;
        }

        public async Task<Message?> ProgressLesson(Lesson lesson)
        {            
            if (lesson.MessagesCount == 0)
                return await StartLesson(lesson);

            return await AddNextSentenceFromBook(lesson);
        }

        Random random = new Random();

        private async Task<Message?> StartLesson(Lesson lesson)
        {
            var startNum = random.Next(0, _booksService.GetSentenceCount(lesson.Scenario.TranslatedBookFile));
            return await AddSentenceFromBook(lesson, startNum);
        }

        private async Task<Message?> AddSentenceFromBook(Lesson lesson, int sentenceNum)
        {
            var sentence = _booksService.GetSentence(sentenceNum, lesson.Scenario.TranslatedBookFile);
            var translated = await _sentenceTranslator.Translate(sentence, "English", "Russian");
            var message = new Message()
            {
                MessageId = Guid.NewGuid(),
                Content = translated,
                MessageType = Entities.Enums.MessageType.GPTMessage,
            };
            lesson.Messages.Add(message);
            _dbContext.Add(message);
            lesson.ProgressInfo = sentenceNum.ToString();
            await _dbContext.SaveChangesAsync();
            return message;
        }

        private async Task<Message?> AddNextSentenceFromBook(Lesson lesson)
        {
            // получить номер, увеличить если книга не закончилась, запросить фразу по номеру, подготовить для перевода
            var sentenceNum = int.Parse(lesson.ProgressInfo!);
            sentenceNum++;
            if (sentenceNum > _booksService.GetSentenceCount(lesson.Scenario.TranslatedBookFile))
                return null;
            return await AddSentenceFromBook(lesson, sentenceNum);            
        }

        public async Task<Message?> ToNextSection(Lesson lesson, ScenarioTemplate scenario)
        {
            var message = await ProgressLesson(lesson);
            return message;
        }
    }
}
