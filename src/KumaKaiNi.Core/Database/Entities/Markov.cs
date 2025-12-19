using System.ComponentModel.DataAnnotations.Schema;
using KumaKaiNi.Core.Models;

namespace KumaKaiNi.Core.Database.Entities;

[Table("markov")]
public class Markov(SourceSystem sourceSystem, string? channelId, string previousWords, string nextWord, long count = 0, bool canStart = false) : BaseDbEntity
{
    [Column("source_system")]
    public SourceSystem SourceSystem { get; set; } = sourceSystem;

    [Column("channel_id")]
    public string? ChannelId { get; set; } = channelId;

    [Column("previous_words")]
    public string PreviousWords { get; set; } = previousWords;

    [Column("next_word")]
    public string NextWord { get; set; } = nextWord;

    [Column("count")]
    public long Count { get; set; } = count;

    [Column("can_start")]
    public bool CanStart { get; set; } = canStart;
}