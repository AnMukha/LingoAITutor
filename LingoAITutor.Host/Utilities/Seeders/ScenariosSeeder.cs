using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;

namespace LingoAITutor.Host.Utilities.Seeders
{
    public class ScenariosSeeder
    {
        LingoDbContext _context;
        string _path;
        public ScenariosSeeder(LingoDbContext context)
        {
            _context = context;
        }

        public void Seed(string path)
        {
            _path = path;
            var sc = new ScenarioTemplate[]
            {
                CreateOpenChat(),
                CreateHPTranslate(),
                CreateNetQuestions()
            };
            foreach (var newSc in sc)
            {
                var existedSc = _context.ScenarioTemplates.FirstOrDefault(s => s.Title == newSc.Title);
                if (existedSc is null)
                    _context.ScenarioTemplates.Add(newSc);
                else
                {
                    existedSc.AIModeInChat = newSc.AIModeInChat;
                    existedSc.Preface = newSc.Preface;
                    existedSc.Content = newSc.Content;
                    existedSc.Title = newSc.Title;
                    existedSc.ScenarioType = newSc.ScenarioType;
                    existedSc.Description = newSc.Description;
                    existedSc.NextQuestionRandom = newSc.NextQuestionRandom;
                    existedSc.TranslatedBookFile = newSc.TranslatedBookFile;
                }
            }            
            _context.SaveChanges();
        }

        private ScenarioTemplate CreateNetQuestions()
        {
            var s = new ScenarioTemplate();
            s.Id = Guid.NewGuid();
            s.Title = ".Net developer interview";
            s.Description = "Chat will ask your to decsribe what you know on subject and then give you feedback how comperhensive and correct was you answer" +
                            "This let you to prepare yourself to interview in English." +
                            "If you really need samrt feedback you can choose the most smpart mode of GTP (be careful, it can be expensive).";
            s.Preface = null;
            s.AIModeInChat = Entities.Enums.AIMode.Good;
            s.ScenarioType = Entities.Enums.ScenarioType.Questions;
            s.Content = ReadFile(Path.Combine(_path, "questions\\NetQuestions.txt"));
            s.NextQuestionRandom = true;
            return s;
        }

        private string ReadFile(string file)
        {
            return File.ReadAllText(file);
        }

        private ScenarioTemplate CreateHPTranslate()
        {
            var s = new ScenarioTemplate();
            s.Id = Guid.NewGuid();
            s.Title = "Translate HP and m.o.r. book";
            s.Description = "At this lesson you should translate the book \"Harry Potter and the Methods of Rationality\"." +
                            "This book laguage is rich enough but mostly free from specipic rare words." +
                            "This lesson choose a random place in this book ant then you can start to translate it from your native languate to stydied language.";
            s.AIModeInChat = Entities.Enums.AIMode.Middle;
            s.ScenarioType = Entities.Enums.ScenarioType.Translation;
            s.Content = null;
            s.TranslatedBookFile = "txt/books/hpmor.txt";
            return s;
        }

        private ScenarioTemplate CreateOpenChat()
        {
            var s = new ScenarioTemplate();
            s.Id = Guid.NewGuid();
            s.Title = "Open discussion chat";
            s.Description = "This is basic chat where you can discuss any topic with ChatGPT. It recomended to use" +
                            " when you really need to study something with chat GPT. Instead using GPT on OpenAI site you can do this here to get train you English at same time." +
                            "To make this useful you can choose the most smpart mode of GTP (be careful, it can be expensive).";
            s.AIModeInChat = Entities.Enums.AIMode.Good;
            s.ScenarioType = Entities.Enums.ScenarioType.FreeChat;
            s.Content = null;            
            return s;
        }
    }
}
