using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace KumaKaiNi.Core
{
    public static class Admin
    {
        [Command("init")]
        public static Response InitDatabase()
        {
            Database.CreateTable<CustomCommand>();
            Database.CreateTable<Error>();
            Database.CreateTable<Log>();
            Database.CreateTable<Quote>();

            return new Response("That probably worked.");
        }

        [Command("drop")]
        public static Response DropDatabase()
        {
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

            Database.DropTable<CustomCommand>();
            Database.CreateTable<CustomCommand>();

            string commandsPath = $"{folder}commands.csv";
            using (StreamReader reader = new StreamReader(commandsPath))
            using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                IEnumerable<dynamic> commands = csv.GetRecords<dynamic>();

                foreach (dynamic command in commands)
                {
                    CustomCommand newCommand = new CustomCommand()
                    {
                        Command = command.command,
                        Response = command.response
                    };

                    newCommand.Insert();
                }
            }

            Database.DropTable<Quote>();
            Database.CreateTable<Quote>();

            string quotesPath = $"{folder}quotes.csv";
            using (StreamReader reader = new StreamReader(quotesPath))
            using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
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

            Database.DropTable<Log>();
            Database.CreateTable<Log>();

            string logsPath = $"{folder}logs.csv";
            using (StreamReader reader = new StreamReader(logsPath))
            using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
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

            return new Response("Should be good to go!");
        }
    }
}
