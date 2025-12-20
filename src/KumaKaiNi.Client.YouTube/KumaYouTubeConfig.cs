namespace KumaKaiNi.Client.YouTube;

public static class KumaYouTubeConfig
{
    public static string? ClientId { get; private set; }

    public static string? ClientSecret { get; private set; }

    public static string? OAuth2Code { get; private set; }

    /// <summary>
    /// Lasts an hour
    /// </summary>
    public static string? AccessToken { get; private set; }

    /// <summary>
    /// Lasts 7 days
    /// </summary>
    public static string? RefreshToken { get; private set; }

    static KumaYouTubeConfig()
    {
        ClientId = Environment.GetEnvironmentVariable("YOUTUBE_CLIENT_ID");
        ClientSecret = Environment.GetEnvironmentVariable("YOUTUBE_CLIENT_SECRET");
        OAuth2Code = Environment.GetEnvironmentVariable("YOUTUBE_OAUTH2_CODE");
        AccessToken = Environment.GetEnvironmentVariable("YOUTUBE_ACCESS_TOKEN");
        RefreshToken = Environment.GetEnvironmentVariable("YOUTUBE_REFRESH_TOKEN");
    }
}