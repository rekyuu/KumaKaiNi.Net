namespace KumaKaiNi.Core.Models;

public class KumaRequest
{
    /// <summary>
    /// The requester's username.
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// The requester's original message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// The system the message request came from.
    /// </summary>
    public SourceSystem SourceSystem { get; set; }

    /// <summary>
    /// The message ID from the source system.
    /// </summary>
    public long MessageId { get; set; }

    /// <summary>
    /// The user's permission authority.
    /// </summary>
    public UserAuthority UserAuthority { get; set; }

    /// <summary>
    /// The channel ID from the source system.
    /// </summary>
    public long? ChannelId { get; set; }

    /// <summary>
    /// Indicates if the source channel was private, such as a direct message.
    /// </summary>
    public bool ChannelIsPrivate { get; set; }
    
    /// <summary>
    /// Indicates if the source channel allows NSFW content.
    /// </summary>
    public bool ChannelIsNsfw { get; set; }

    /// <summary>
    /// Indicates if the user provided a ! command at the start of their message.
    /// </summary>
    public bool IsCommand { get; private set; } = false;

    /// <summary>
    /// The message's command, if it exists.
    /// </summary>
    public string? Command { get; private set; }

    /// <summary>
    /// The command's arguments, if it was a command.
    /// </summary>
    public string[] CommandArgs { get; private set; } = [];

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
        long messageId = 0,
        UserAuthority userAuthority = UserAuthority.User,
        long channelId = 0,
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
        
        // Determine if the incoming message was a command or not
        if (string.IsNullOrEmpty(Message) || Message[0] != '!') return;
        
        string[] messageContents = Message.Split(' ');

        IsCommand = true;
        Command = messageContents[0][1..];
        CommandArgs = messageContents.Skip(1).ToArray();
    }
}

