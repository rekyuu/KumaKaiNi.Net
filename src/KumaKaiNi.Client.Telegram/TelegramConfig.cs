namespace KumaKaiNi.Client.Telegram;

public static class KumaTelegramConfig
{
    /// <summary>
    /// The Telegram bot access token.
    /// </summary>
    public static string? TelegramAccessToken { get; private set; }
    
    /// <summary>
    /// The user ID of the administrator.
    /// </summary>
    public static long? TelegramAdminId { get; private set; }

    static KumaTelegramConfig()
    {
        TelegramAccessToken = Environment.GetEnvironmentVariable("TELEGRAM_ACCESS_TOKEN");

        string? telegramAdminId = Environment.GetEnvironmentVariable("TELEGRAM_ADMIN_ID");
        if (!string.IsNullOrEmpty(telegramAdminId)) TelegramAdminId = long.Parse(telegramAdminId);
    }
}