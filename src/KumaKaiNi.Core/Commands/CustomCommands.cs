using System;
using System.Collections.Generic;

namespace KumaKaiNi.Core
{
    [RequireModerator]
    public static class CustomCommands
    {
        [Command("command")]
        public static Response CustomCommand(Request request)
        {
            const string errorResponse = "Usage: !command [add|edit|del] <command> <response>";

            if (request.CommandArgs.Length == 0) return new Response(errorResponse);

            string command;
            string result;

            switch (request.CommandArgs[0])
            {
                case "add":
                case "new":
                case "edit":
                case "mod":
                case "modify":
                    if (request.CommandArgs.Length < 3) return new Response(errorResponse);

                    command = request.CommandArgs[1];
                    string response = string.Join(" ", request.CommandArgs[2..]);

                    result = AddOrEditCommand(command, response);
                    return new Response(result);
                case "del":
                case "delete":
                case "rem":
                case "remove":
                    if (request.CommandArgs.Length < 2) return new Response(errorResponse);

                    command = request.CommandArgs[1];

                    result = RemoveCommand(command);
                    return new Response(result);
                default:
                    return new Response(errorResponse);
            }
        }

        private static string AddOrEditCommand(string command, string response)
        {
            if (command[0] == '!') command = command[1..];

            WherePredicate where = new WherePredicate()
            {
                Source = "command",
                Comparitor = "=",
                Target = command
            };

            List<CustomCommand> customCommands = Database.GetMany<CustomCommand>(new[] { where });
            if (customCommands.Count == 1)
            {
                customCommands[0].Response = response;
                customCommands[0].Update();

                return $"Command !{command} updated.";
            }

            if (customCommands.Count > 1)
            {
                throw new Exception($"There are multiple custom commands of the same name: {command}");
            }

            CustomCommand newCustomCommand = new CustomCommand()
            {
                Command = command,
                Response = response
            };

            newCustomCommand.Insert();

            return $"Command !{command} added.";
        }

        private static string RemoveCommand(string command)
        {
            if (command[0] == '!') command = command[1..];

            WherePredicate where = new WherePredicate()
            {
                Source = "command",
                Comparitor = "=",
                Target = command
            };

            List<CustomCommand> customCommands = Database.GetMany<CustomCommand>(new[] { where });
            if (customCommands.Count == 1)
            {
                customCommands[0].Delete();

                return $"Command !{command} removed.";
            }

            if (customCommands.Count > 1)
            {
                throw new Exception($"There are multiple custom commands of the same name: {command}");
            }
            
            return "";
        }
    }
}
