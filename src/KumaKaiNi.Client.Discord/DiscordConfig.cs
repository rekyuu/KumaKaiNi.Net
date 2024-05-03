namespace KumaKaiNi.Discord;

public static class KumaDiscordConfig
{
    /// <summary>
    /// The Discord Bot API token.
    /// </summary>
    public static string? DiscordToken { get; private set; }
    
    /// <summary>
    /// The role ID that for moderators.
    /// </summary>
    public static ulong? DiscordModRoleId { get; private set; }
    
    /// <summary>
    /// The user ID of the administrator.
    /// </summary>
    public static ulong? DiscordAdminId { get; private set; }
    
    /// <summary>
    /// The guild ID to update the moon phase guild image.
    /// </summary>
    public static ulong? DiscordMoonGuildId { get; private set; }

    static KumaDiscordConfig()
    {
        DiscordToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

        string? discordModRoleId = Environment.GetEnvironmentVariable("DISCORD_MOD_ROLE_ID");
        if (!string.IsNullOrEmpty(discordModRoleId)) DiscordModRoleId = ulong.Parse(discordModRoleId);

        string? discordAdminId = Environment.GetEnvironmentVariable("DISCORD_ADMIN_ID");
        if (!string.IsNullOrEmpty(discordAdminId)) DiscordAdminId = ulong.Parse(discordAdminId);

        string? discordMoonGuildId = Environment.GetEnvironmentVariable("DISCORD_MOON_GUILD_ID");
        if (!string.IsNullOrEmpty(discordMoonGuildId)) DiscordMoonGuildId = ulong.Parse(discordMoonGuildId);
    }
}