namespace KumaKaiNi.Client.Twitch;

public static class KumaTwitchConfig
{
    /// <summary>
    /// The Twitch channel to join.
    /// </summary>
    public static string? TwitchChannel { get; private set; }

    /// <summary>
    /// The Twitch bot username.
    /// </summary>
    public static string? TwitchUsername { get; private set; }

    /// <summary>
    /// The Twitch bot access token.
    /// </summary>
    public static string? TwitchAccessToken { get; private set; }

    static KumaTwitchConfig()
    {
        TwitchChannel = Environment.GetEnvironmentVariable("TWITCH_CHANNEL");
        TwitchUsername = Environment.GetEnvironmentVariable("TWITCH_USERNAME");
        TwitchAccessToken = Environment.GetEnvironmentVariable("TWITCH_ACCESS_TOKEN");
    }
}