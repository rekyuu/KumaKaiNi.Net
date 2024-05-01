using KumaKaiNi.Core.Attributes;
using KumaKaiNi.Core.Database;
using KumaKaiNi.Core.Database.Entities;
using KumaKaiNi.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KumaKaiNi.Core.Commands;

public static class CustomCommands
{
    private const string? ErrorResponse = "Usage: !command [add|edit|del] <command> <response>";
    
    [Command("command", UserAuthority.Moderator)]
    public static async Task<KumaResponse> CustomCommandAsync(KumaRequest kumaRequest)
    {
        if (kumaRequest.CommandArgs.Length == 0) return new KumaResponse(ErrorResponse);

        string command;
        string? result;

        switch (kumaRequest.CommandArgs[0])
        {
            case "add":
            case "new":
            case "edit":
            case "mod":
            case "modify":
                if (kumaRequest.CommandArgs.Length < 3) return new KumaResponse(ErrorResponse);

                command = kumaRequest.CommandArgs[1];
                string response = string.Join(" ", kumaRequest.CommandArgs[2..]);
                result = await AddOrEditCommandAsync(command, response);
                
                return new KumaResponse(result);
            case "del":
            case "delete":
            case "rem":
            case "remove":
                if (kumaRequest.CommandArgs.Length < 2) return new KumaResponse(ErrorResponse);

                command = kumaRequest.CommandArgs[1];
                result = await RemoveCommandAsync(command);
                
                return new KumaResponse(result);
            default:
                return new KumaResponse(ErrorResponse);
        }
    }

    private static async Task<string> AddOrEditCommandAsync(string command, string response)
    {
        if (command[0] == '!') command = command[1..];

        // Get any matching custom commands
        await using KumaKaiNiDbContext db = new();
        List<CustomCommand> customCommands = await db.CustomCommands
            .Where(x => x.Command == command)
            .ToListAsync();

        string result;
        switch (customCommands.Count)
        {
            // Update the command
            case 1:
                customCommands.First().Response = response;
                result = $"Command !{command} updated.";
                
                break;
            // Throw if there's somehow more than one command
            case > 1:
                throw new Exception($"There are multiple custom commands of the same name: {command}");
            // Command doesn't exist yet, create it
            default:
                CustomCommand newCustomCommand = new(command, response);
                
                await db.CustomCommands.AddAsync(newCustomCommand);
                result = $"Command !{command} added.";
                
                break;
        }

        await db.SaveChangesAsync();
        return result;
    }

    private static async Task<string?> RemoveCommandAsync(string command)
    {
        if (command[0] == '!') command = command[1..];

        await using KumaKaiNiDbContext db = new();
        List<CustomCommand> customCommands = await db.CustomCommands
            .Where(x => x.Command == command)
            .ToListAsync();

        if (customCommands.Count == 0) return null;
        
        foreach (CustomCommand customCommand in customCommands)
        {
            db.CustomCommands.Remove(customCommand);
        }

        await db.SaveChangesAsync();
        return $"Command !{command} removed.";
    }
}