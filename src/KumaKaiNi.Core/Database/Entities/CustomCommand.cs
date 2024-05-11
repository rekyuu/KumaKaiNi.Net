using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

[Table("custom_commands")]
public class CustomCommand(string command, string response) : BaseDbEntity
{
    [Column("command")]
    public string Command { get; set; } = command;

    [Column("response")]
    public string Response { get; set; } = response;
}
