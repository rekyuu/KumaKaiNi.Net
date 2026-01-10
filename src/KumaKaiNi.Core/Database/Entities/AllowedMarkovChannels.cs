using System.ComponentModel.DataAnnotations.Schema;

namespace KumaKaiNi.Core.Database.Entities;

public class AllowedMarkovChannels(string channelId) : BaseDbEntity
{
    [Column("channel_id")]
    public string ChannelId { get; set; } = channelId;
}