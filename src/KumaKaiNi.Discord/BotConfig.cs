using System;

namespace KumaKaiNi.Discord
{
    public static class BotConfig
    {
        public static string DiscordToken => Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        
        public static string DiscordModRoleId => Environment.GetEnvironmentVariable("DISCORD_MOD_ROLE_ID");
        
        public static string DiscordAdminId => Environment.GetEnvironmentVariable("DISCORD_ADMIN_ID");
    }
}