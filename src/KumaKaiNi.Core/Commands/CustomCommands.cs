using System;
using System.Collections.Generic;
using System.Text;

namespace KumaKaiNi.Core
{
    [RequireModerator]
    public static class CustomCommands
    {
        [Command("command")]
        public static Response CustomCommand(Request request)
        {
            string errorResponse = "Usage: !command [add|edit|del] <command> <response>";

            if (request.CommandArgs.Length == 0) return new Response(errorResponse);

            string command;
            string response;
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
                    response = string.Join(" ", request.CommandArgs[2..]);

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

        public static string AddOrEditCommand(string command, string response)
        {
            if (command[0] == '!') command = command[1..];

            WherePredicate where = new WherePredicate()
            {
                Source = "command",
                Comparitor = "=",
                Target = command
            };

            List<CustomCommand> customCommands = Database.GetMany<CustomCommand>(new WherePredicate[] { where });
            if (customCommands.Count == 1)
            {
                customCommands[0].Response = response;
                customCommands[0].Update();

                return $"Command !{command} updated.";
            }
            else if (customCommands.Count > 1) throw new Exception($"There are multiple custom commands of the same name: {command}");
            else
            {
                CustomCommand newCustomCommand = new CustomCommand()
                {
                    Command = command,
                    Response = response
                };

                newCustomCommand.Insert();

                return $"Command !{command} added.";
            }
        }

        public static string RemoveCommand(string command)
        {
            if (command[0] == '!') command = command[1..];

            WherePredicate where = new WherePredicate()
            {
                Source = "command",
                Comparitor = "=",
                Target = command
            };

            List<CustomCommand> customCommands = Database.GetMany<CustomCommand>(new WherePredicate[] { where });
            if (customCommands.Count == 1)
            {
                customCommands[0].Delete();

                return $"Command !{command} removed.";
            }
            else if (customCommands.Count > 1) throw new Exception($"There are multiple custom commands of the same name: {command}");
            else return "";
        }
    }
}
