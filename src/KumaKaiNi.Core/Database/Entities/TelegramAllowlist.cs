using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

[Table("telegram_allowlists")]
public class TelegramAllowList(string channelId) : BaseDbEntity
{
    [Column("channel_id")]
    public string ChannelId { get; set; } = channelId;

    [Column("approved")]
    public bool Approved { get; set; } = false;

    [Column("warnings")]
    public int Warnings { get; set; } = 0;
}
