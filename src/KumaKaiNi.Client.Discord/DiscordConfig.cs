namespace KumaKaiNi.Client.Discord;

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
        bool discordModRoleIdParsed = ulong.TryParse(discordModRoleId, out ulong discordModRoleIdResult);
        if (discordModRoleIdParsed) DiscordModRoleId = discordModRoleIdResult;

        string? discordAdminId = Environment.GetEnvironmentVariable("DISCORD_ADMIN_ID");
        bool discordAdminIdParsed = ulong.TryParse(discordAdminId, out ulong discordAdminIdResult);
        if (discordAdminIdParsed) DiscordModRoleId = discordAdminIdResult;

        string? discordMoonGuildId = Environment.GetEnvironmentVariable("DISCORD_MOON_GUILD_ID");
        bool discordMoonGuildIdParsed = ulong.TryParse(discordMoonGuildId, out ulong discordMoonGuildIdResult);
        if (discordMoonGuildIdParsed) DiscordModRoleId = discordMoonGuildIdResult;
    }
}