using System.ComponentModel.DataAnnotations.Schema;
using KumaKaiNi.Core.Models;

namespace KumaKaiNi.Core.Database.Entities;

[Table("chat_logs")]
public class ChatLog(DateTime timestamp, SourceSystem sourceSystem, string message, string? messageId, string username, string? channelId, bool @private) : BaseDbEntity
{
    [Column("timestamp", TypeName = "timestamp without time zone")]
    public DateTime Timestamp { get; set; } = timestamp;

    [Column("source_system")]
    public SourceSystem SourceSystem { get; set; } = sourceSystem;

    [Column("message")]
    public string Message { get; set; } = message;

    [Column("message_id")]
    public string? MessageId { get; set; } = messageId;

    [Column("username")]
    public string Username { get; set; } = username;

    [Column("channel_id")]
    public string? ChannelId { get; set; } = channelId;

    [Column("private")]
    public bool Private { get; set; } = @private;
}
