using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

[Table("gpt_responses")]
public class GptResponse(string message, bool returned = false) : BaseDbEntity
{
    [Column("message")]
    public string Message { get; set; } = message;

    [Column("returned")]
    public bool Returned { get; set; } = returned;
}
