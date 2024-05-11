using System.Text.Json.Serialization;

namespace KumaKaiNi.Core.Models;

public class KumaRequest
{
    /// <summary>
    /// The requester's username.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; }
    
    /// <summary>
    /// The requester's original message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    /// <summary>
    /// The system the message request came from.
    /// </summary>
    [JsonPropertyName("source_system")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SourceSystem SourceSystem { get; set; }

    /// <summary>
    /// The message ID from the source system.
    /// </summary>
    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }

    /// <summary>
    /// The user's permission authority.
    /// </summary>
    [JsonPropertyName("user_authority")]
    public UserAuthority UserAuthority { get; set; }

    /// <summary>
    /// The channel ID from the source system.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    /// <summary>
    /// Indicates if the source channel was private, such as a direct message.
    /// </summary>
    [JsonPropertyName("channel_is_private")]
    public bool ChannelIsPrivate { get; set; }
    
    /// <summary>
    /// Indicates if the source channel allows NSFW content.
    /// </summary>
    [JsonPropertyName("channel_is_nsfw")]
    public bool ChannelIsNsfw { get; set; }

    /// <summary>
    /// The date and time when the request was created.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates if the user provided a ! command at the start of their message.
    /// </summary>
    [JsonIgnore]
    public bool IsCommand { get; private set; } = false;

    /// <summary>
    /// The message's command, if it exists.
    /// </summary>
    [JsonIgnore]
    public string? Command { get; private set; }

    /// <summary>
    /// The command's arguments, if it was a command.
    /// </summary>
    [JsonIgnore]
    public string[] CommandArgs { get; private set; } = [];
    
    [JsonConstructor]
    public KumaRequest(string username, string message)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(message);
        
        Username = username;
        Message = message;

        Init();
    }

    /// <summary>
    /// Object used to issue requests to Kuma.
    /// </summary>
    /// <param name="username">The requester's username.</param>
    /// <param name="message">The requester's original message.</param>
    /// <param name="sourceSystem">The system the message request came from.</param>
    /// <param name="messageId">The message ID from the source system.</param>
    /// <param name="userAuthority">The user's permission authority.</param>
    /// <param name="channelId">The channel ID from the source system.</param>
    /// <param name="channelIsPrivate">Indicates if the source channel was private, such as a direct message.</param>
    /// <param name="channelIsNsfw">Indicates if the source channel allows NSFW content.</param>
    public KumaRequest(
        string username,
        string message,
        SourceSystem sourceSystem,
        string? messageId = null,
        UserAuthority userAuthority = UserAuthority.User,
        string? channelId = null,
        bool channelIsPrivate = false,
        bool channelIsNsfw = false)
    {
        Username = username;
        Message = message;
        SourceSystem = sourceSystem;
        MessageId = messageId;
        UserAuthority = userAuthority;
        ChannelId = channelId;
        ChannelIsPrivate = channelIsPrivate;
        ChannelIsNsfw = channelIsNsfw;

        Init();
    }

    private void Init()
    {
        // Determine if the incoming message was a command or not
        if (string.IsNullOrEmpty(Message) || Message[0] != '!') return;
        
        string[] messageContents = Message.Split(' ');

        IsCommand = true;
        Command = messageContents[0][1..];
        CommandArgs = messageContents.Skip(1).ToArray();
    }
}

