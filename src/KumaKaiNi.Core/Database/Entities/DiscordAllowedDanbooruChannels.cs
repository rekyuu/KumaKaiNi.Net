using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

public class DiscordAllowedDanbooruChannel(string channelId) : BaseDbEntity
{
    [Column("channel_id")]
    public string ChannelId { get; set; } = channelId;
}