namespace KumaKaiNi.Client.Telegram;

public static class KumaTelegramConfig
{
    /// <summary>
    /// The Telegram bot access token.
    /// </summary>
    public static string? TelegramAccessToken { get; private set; }

    static KumaTelegramConfig()
    {
        TelegramAccessToken = Environment.GetEnvironmentVariable("TELEGRAM_ACCESS_TOKEN");
    }
}