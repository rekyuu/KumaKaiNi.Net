using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KumaKaiNi.Core
{
    [RequireAdmin]
    public static class Admin
    {
        [Command("danban")]
        public static Response BlockTags(Request request)
        {
            List<DanbooruBlocklist> blocklist = Database.GetMany<DanbooruBlocklist>();
            string[] blockedTags = new string[blocklist.Count];

            int i = 0;
            foreach (DanbooruBlocklist entry in blocklist)
            {
                blockedTags[i] = entry.Tag;
                i++;
            }

            int inserted = 0;
            foreach (string tag in request.CommandArgs)
            {
                if (blockedTags.Contains(tag)) continue;

                DanbooruBlocklist newTag = new DanbooruBlocklist() { Tag = tag };
                newTag.Insert();
                inserted++;
            }

            if (inserted > 0) return new Response($"Tags added.");
            else return new Response("Nothing to add.");
        }

        [Command("danunban")]
        public static Response AllowTags(Request request)
        {
            List<DanbooruBlocklist> blocklist = Database.GetMany<DanbooruBlocklist>();
            Dictionary<string, DanbooruBlocklist> blockedTags = new Dictionary<string, DanbooruBlocklist>();

            foreach (DanbooruBlocklist entry in blocklist)
            {
                blockedTags.Add(entry.Tag, entry);
            }

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
            Database.CreateTable<DanbooruBlocklist>();
            Database.CreateTable<DanbooruCache>();
            Database.CreateTable<CustomCommand>();
            Database.CreateTable<Error>();
            Database.CreateTable<Log>();
            Database.CreateTable<Quote>();

            return new Response("That probably worked.");
        }

        [Command("drop")]
        public static Response DropDatabase()
        {
            Database.DropTable<DanbooruBlocklist>();
            Database.DropTable<DanbooruCache>();
            Database.DropTable<CustomCommand>();
            Database.DropTable<Error>();
            Database.DropTable<Log>();
            Database.DropTable<Quote>();

            return new Response("Everyone is dead.");
        }

        [Command("restore")]
        public static Response Restore(Request request)
        {
            string folder = @"C:\KumaKaiNiMigration\";
            if (request.CommandArgs.Length > 0) folder = request.CommandArgs[0];

            ProcessCustomCommands(folder);
            ProcessQuotes(folder);
            ProcessLogs(folder);

            return new Response("Should be good to go!");
        }

        private static void ProcessCustomCommands(string folder)
        {
            Database.DropTable<CustomCommand>();
            Database.CreateTable<CustomCommand>();

            string commandsPath = $"{folder}commands.csv";
            using StreamReader reader = new StreamReader(commandsPath);
            using CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            IEnumerable<dynamic> commands = csv.GetRecords<dynamic>();

            foreach (dynamic command in commands)
            {
                CustomCommand newCommand = new CustomCommand()
                {
                    Command = (string)(command.command).Replace("!", ""),
                    Response = command.response
                };

                newCommand.Insert();
            }
        }

        private static void ProcessQuotes(string folder)
        {
            Database.DropTable<Quote>();
            Database.CreateTable<Quote>();

            string quotesPath = $"{folder}quotes.csv";
            using StreamReader reader = new StreamReader(quotesPath);
            using CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            IEnumerable<dynamic> quotes = csv.GetRecords<dynamic>();

            foreach (dynamic quote in quotes)
            {
                Quote newQuote = new Quote()
                {
                    Text = quote.text
                };

                newQuote.Insert();
            }
        }

        private static void ProcessLogs(string folder)
        {
            Database.DropTable<Log>();
            Database.CreateTable<Log>();

            string logsPath = $"{folder}logs.csv";
            using StreamReader reader = new StreamReader(logsPath);
            using CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Configuration.BadDataFound = null;

            IEnumerable<dynamic> logs = csv.GetRecords<dynamic>();
            foreach (dynamic log in logs)
            {
                try
                {
                    if (log.message != "")
                    {
                        Log newLog = new Log()
                        {
                            Timestamp = DateTime.Parse(log.timestamp, null, DateTimeStyles.RoundtripKind),
                            Protocol = (RequestProtocol)Enum.Parse(typeof(RequestProtocol), log.protocol),
                            Message = log.message,
                            Username = log.username
                        };

                        try
                        {
                            newLog.ChannelId = long.Parse(log.channel);
                        }
                        catch { }

                        newLog.Insert();
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                }
            }
        }
    }
}
