using System;

namespace KumaKaiNi.Twitch
{
    public static class BotConfig
    {
        public static string TwitchAccessToken => Environment.GetEnvironmentVariable("TWITCH_ACCESS_TOKEN");
    }
}