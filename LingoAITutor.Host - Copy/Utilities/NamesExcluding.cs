using LingoAITutor.Host.Infrastructure;
using Microsoft.AspNetCore.HttpLogging;
using OpenAI_API;
using OpenAI_API.Chat;    
using OpenAI_API.Models;

namespace LingoAITutor.Host.Utilities
{
    public class NamesExcluding
    {
        private LingoDbContext _context;
        private OpenAIAPI _openAPI;

        public NamesExcluding(LingoDbContext context, OpenAIAPI openAPI)
        {
            _context = context;
            _openAPI = openAPI;
        }

        public async Task<int> ProcessNamesFiles()
        {
            var names1 = File.ReadAllLines("c:/!/es_forms.txt");
            var names2 = File.ReadAllLines("c:/!/es_forms_ru.txt");
            var result1 = new List<string>();
            var result2 = new List<string>();
            for (var i = 0; i < names2.Length; i++)
            {
                var parts1 = names1[i].Split(',');
                if (parts1.Length < 2)
                    parts1 = names1[i].Split(' ');
                if (parts1.Length < 2)
                    continue;
                var parts2 = names2[i].Split(',');
                if (parts2.Length < 2)
                    parts2 = names2[i].Split(' ');
                if (parts2.Length < 2)
                    continue;
                var w1 = parts2[0].TrimEnd();
                var w2 = parts2[1].TrimStart();
                if (w1.Length < 2 || w2.Length < 2)
                    continue;
                if ((w1.Length <= 5 || w2.Length <= 5))
                {
                    if (w1.Substring(0, 2) != w2.Substring(0, 2))
                    {
                        result1.Add(parts1[0]);
                        result2.Add(names2[i]);
                    }
                }
                else
                if (w1.Substring(0, 3) != w2.Substring(0, 3))
                {
                    result1.Add(parts1[0]);
                    result2.Add(names2[i]);
                }
            }
            File.WriteAllLines("c:/!/not_es_words.txt", result1);
            File.WriteAllLines("c:/!/not_es_words_ru.txt", result2);
            return 0;

            // составить список слов, которые нельзя объединять, остальные будут объединены

            File.WriteAllLines("c:/!/propnames.txt", names2);
            return 0;

            var names = File.ReadAllLines("c:/!/names.txt");
            var ruNames = File.ReadAllLines("c:/!/runames.txt");
            var resultNames = new List<string>();
            var sky = new List<string>();
            for(var i = 0; i< names.Length; i++)
            {
                if (ruNames[i].Substring(0, 1).ToLower() != ruNames[i].Substring(0, 1))
                    resultNames.Add(names[i]);
                else
                {
                    if (ruNames[i].EndsWith("ский"))
                        sky.Add(names[i] + "," + ruNames[i]);
                }   
            }
            File.WriteAllLines("c:/!/final_names.txt", resultNames);
            File.WriteAllLines("c:/!/sky_names.txt", sky);
            return 0;            
        }

        public async Task<int> FindNamesInWordsWithGPT()
        {
            var words = _context.Words.OrderBy(w => w.FrequencyRank).Select(w => w.Text).ToArray();
            for (var i = 12000; i < words.Length; i = i + 300)
            {
                try
                {
                    var last = Math.Min(i + 300, words.Length - 1);
                    var portion = words[i..last];
                    await ProcessWords(portion);
                }
                catch (Exception e)
                {
                    var ii = 2;
                }
            }
            return 0;
        }

        private async Task ProcessWords(string[] portion)
        {
            var result = await _openAPI.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.GPT4,
                Temperature = 0,
                MaxTokens = 1000,
                Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.User,
                                    "Select proper nouns from this list of words. Please, write the result noun list without any comments. /n"+
                                    string.Join("/n", portion)+"johnson")
                }
            });

            var resultText = result.Choices[0].Message.Content;
            var words = resultText.Split(", ").Select(w => w.ToLower()).Where(w => w!= "johnson").ToArray();

            File.AppendAllLines("c:/!/names.txt", words);

        }
    }
}
