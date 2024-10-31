using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

public class AdminConfig : BaseDbEntity
{
    [Column("open_ai_model")]
    public string OpenAiModel { get; set; } = "gpt-4-turbo";

    [Column("open_ai_token_limit")]
    public long OpenAiTokenLimit { get; set; } = 2048;
}