using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KumaKaiNi.Core
{
    [RequireAdmin]
    public static class Admin
    {
        [Command("danban")]
        public static Response BlockTags(Request request)
        {
            List<DanbooruBlockList> blockList = Database.GetMany<DanbooruBlockList>();
            string[] blockedTags = new string[blockList.Count];

            int i = 0;
            foreach (DanbooruBlockList entry in blockList)
            {
                blockedTags[i] = entry.Tag;
                i++;
            }

            int inserted = 0;
            foreach (string tag in request.CommandArgs)
            {
                if (blockedTags.Contains(tag)) continue;

                DanbooruBlockList newTag = new DanbooruBlockList() { Tag = tag };
                newTag.Insert();
                inserted++;
            }

            return inserted > 0 ? new Response($"Tags added.") : new Response("Nothing to add.");
        }

        [Command("danunban")]
        public static Response AllowTags(Request request)
        {
            List<DanbooruBlockList> blockList = Database.GetMany<DanbooruBlockList>();
            Dictionary<string, DanbooruBlockList> blockedTags = blockList
                .ToDictionary(entry => entry.Tag);

            int deleted = 0;
            foreach (string tag in request.CommandArgs)
            {
                if (!blockedTags.ContainsKey(tag)) continue;

                blockedTags[tag].Delete();
                deleted++;
            }

            if (deleted > 0) return new Response($"Tags removed.");
            else return new Response("Nothing to remove.");
        }

        [Command("init")]
        public static Response InitDatabase()
        {
            Database.CreateTable<CustomCommand>();
            Database.CreateTable<DanbooruBlockList>();
            Database.CreateTable<DanbooruCache>();
            Database.CreateTable<Error>();
            Database.CreateTable<Log>();
            Database.CreateTable<Quote>();

            return new Response("That probably worked.");
        }

        // [Command("drop")]
        public static Response DropDatabase()
        {
            Database.DropTable<CustomCommand>();
            Database.DropTable<DanbooruBlockList>();
            Database.DropTable<DanbooruCache>();
            Database.DropTable<Error>();
            Database.DropTable<Log>();
            Database.DropTable<Quote>();

            return new Response("Everyone is dead.");
        }

        [Command("dropgpt")]
        public static Response DropGptResponses()
        {
            Database.DropTable<GptResponse>();

            return new Response("Done.");
        }

        [Command("newgpt")]
        public static Response ProcessNewGptResponses()
        {
            Database.CreateTable<GptResponse>();

            const string kumaRootWindows = @"C:\KumaRoot\GPT\";
            string kumaRootLinux = $"/srv/KumaRoot/GPT/";
            string kumaRoot = Helpers.IsLinux() ? kumaRootLinux : kumaRootWindows;
            string newGptDir = kumaRoot + "New";
            string processedGptDir = kumaRoot + "Processed";

            string[] files = Directory.GetFiles(newGptDir);
            int responsesAdded = 0;

            if (files.Length == 0) return new Response("There are no new responses to add.");

            foreach (string sourceFile in files)
            {
                using (StreamReader reader = new StreamReader(sourceFile))
                {
                    string content = reader.ReadToEnd();
                    string[] responses = content.Split("\n====================\n");

                    foreach (string response in responses)
                    {
                        string message = response
                            .Replace("<|startoftext|>", "")
                            .Replace("@everyone", "everyone");
                        string[] words = message.Split(" ");

                        string spamCheck = message;
                        if (words.Length > 1) spamCheck = message.Replace(words[0], "").Replace(" ", "");

                        if (spamCheck.Length > 0 && message.Length <= 2000 && words.Length >= 3)
                        {
                            GptResponse gpt = new GptResponse()
                            {
                                Message = message,
                                Returned = false
                            };

                            gpt.Insert();
                            responsesAdded++;
                        }
                    }
                }

                string filename = Path.GetFileName(sourceFile);
                string destinationFile = Path.Combine(processedGptDir, filename);

                Directory.CreateDirectory(processedGptDir);
                File.Move(sourceFile, destinationFile);
            }

            return new Response($"{responsesAdded} new responses added.");
        }
    }
}
