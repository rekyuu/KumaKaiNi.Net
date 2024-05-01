using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

[Table("telegram_allowlists")]
public class TelegramAllowList : BaseDbEntity
{
    [Column("channel_id")]
    public long ChannelId { get; set; }

    [Column("approved")]
    public bool Approved { get; set; } = false;

    [Column("warnings")]
    public int Warnings { get; set; } = 0;
}
