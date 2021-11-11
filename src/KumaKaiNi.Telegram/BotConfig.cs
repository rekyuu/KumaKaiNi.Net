using System;

namespace KumaKaiNi.Telegram
{
    public static class BotConfig
    {
        public static string TelegramToken => Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
        
        public static int TelegramAdminId => int.Parse(Environment.GetEnvironmentVariable("TELEGRAM_ADMIN_ID"));
    }
}