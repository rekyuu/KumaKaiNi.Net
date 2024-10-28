using System.Text.Json.Serialization;

namespace KumaKaiNi.Core.Models;

public class KumaResponse
{
    /// <summary>
    /// The response message.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message  { get; set; }
    
    /// <summary>
    /// The response image.
    /// </summary>
    [JsonPropertyName("image")]
    public ResponseMedia? Media  { get; set; }

    /// <summary>
    /// The system the message request came from.
    /// </summary>
    [JsonPropertyName("source_system")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SourceSystem SourceSystem { get; set; }
    
    /// <summary>
    /// The response destination channel ID.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    /// <summary>
    /// The date and time when the response was created.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Object used for responding to requests.
    /// </summary>
    public KumaResponse() { }

    /// <summary>
    /// Object used for responding to requests.
    /// </summary>
    /// <param name="message">The response message.</param>
    public KumaResponse(string? message = "")
    {
        Message = message;
    }

    /// <summary>
    /// Object used for responding to requests.
    /// </summary>
    /// <param name="media">The response media.</param>
    public KumaResponse(ResponseMedia media)
    {
        Message = "";
        Media = media;
    }

    /// <summary>
    /// Object used for responding to requests.
    /// </summary>
    /// <param name="message">The response message.</param>
    /// <param name="media">The response media.</param>
    public KumaResponse(string? message, ResponseMedia media)
    {
        Message = message;
        Media = media;
    }

    public override string? ToString()
    {
        if (string.IsNullOrEmpty(Message) && Media != null) return Media.Source;
        return Message;
    }
}