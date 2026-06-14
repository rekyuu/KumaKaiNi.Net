using System.Text.Json;
using System.Timers;
using System.Web;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using KumaKaiNi.Core;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;
using Serilog;
using StackExchange.Redis;
using Timer = System.Timers.Timer;

namespace KumaKaiNi.Client.Discord;

internal static class Program
{
    private static readonly string[] SupportedVideoFileTypes = [ "mp4", "webm" ];

    private static RedisStreamConsumer? _streamConsumer;
    private static DiscordSocketClient? _discordClient;
    private static CancellationTokenSource? _cts;
    private static RequestOptions? _defaultDiscordRequestOptions;

    private static Timer? _moonTimer;
    private static Timer? _mahjongTimer;
    private static Timer? _festiveTimer;

    private static readonly Dictionary<string, string> RoleColors = new()
    {
        {"Red", "❤️"},
        {"Pink", "🩷"},
        {"Orange", "🧡"},
        {"Yellow", "💛"},
        {"Green", "💚"},
        {"Teal", "🩵"},
        {"Blue", "💙"},
        {"Purple", "💜"},
        {"None", "🤍"},
    };
    
    private static async Task Main()
    {
        try
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(KumaRuntimeConfig.GetLogLevel())
                .WriteTo.Console()
                .CreateLogger();
            
            Log.Information("Starting {ApplicationName} {ApplicationVersion} on {MachineName}", 
                KumaRuntimeConfig.ApplicationName, 
                KumaRuntimeConfig.ApplicationVersion, 
                Environment.MachineName);

            Log.Verbose("Verbose logging enabled");
            Log.Debug("Debug logging enabled");

            if (string.IsNullOrEmpty(KumaDiscordConfig.DiscordToken))
            {
                Log.Fatal("DISCORD_TOKEN environment variable must be set, exiting");
                Environment.Exit(1);
            }

            _cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                _cts.Cancel();
                eventArgs.Cancel = true;
            };

            _streamConsumer = new RedisStreamConsumer(
                Redis.GetStreamNameForSourceSystem(SourceSystem.Discord),
                cancellationToken: _cts.Token);

            _streamConsumer.StreamEntryReceived += OnStreamEntryReceived;
            await _streamConsumer.StartAsync();

            DiscordSocketConfig discordConfig = new()
            {
                GatewayIntents = GatewayIntents.MessageContent |
                                 GatewayIntents.Guilds |
                                 GatewayIntents.GuildMessages |
                                 GatewayIntents.GuildMessageReactions
            };

            _discordClient = new DiscordSocketClient(discordConfig);
            _defaultDiscordRequestOptions = new RequestOptions { CancelToken = _cts.Token };
            _discordClient.Log += OnDiscordLog;
            _discordClient.Ready += OnDiscordReady;
            _discordClient.MessageReceived += OnDiscordMessageReceived;
            _discordClient.ReactionAdded += OnDiscordReactionAdded;

            await _discordClient.LoginAsync(TokenType.Bot, KumaDiscordConfig.DiscordToken);
            await _discordClient.StartAsync();

            Log.Information("Listening for updates");
            await Redis.SendDeploymentNotificationToAdmin();
            
            await Task.Delay(-1, _cts.Token);
        }
        catch (TaskCanceledException)
        {
            Log.Information("Exiting");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            await Logging.LogExceptionToDatabaseAsync(ex, "An exception was thrown while starting");
            Environment.Exit(1);
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static Task OnDiscordLog(LogMessage message)
    {
        if (message.Exception != null) Log.Error("{Message}", message);
        else Log.Information("{Message}", message);
        
        return Task.CompletedTask;
    }

    private static Task OnDiscordReady()
    {
        Log.Information("Discord is ready");

        _moonTimer = new Timer(60 * 60 * 1000);
        _moonTimer.Elapsed += OnMoonTimerElapsed;
        _moonTimer.Start();
        
        Log.Information("Started moon phase timer");

        _mahjongTimer = new Timer(60 * 60 * 1000);
        _mahjongTimer.Elapsed += OnMahjongTimerElapsed;
        _mahjongTimer.Start();

        Log.Information("Started mahjong timer");

        _festiveTimer = new Timer(24 * 60 * 60 * 1000);
        _festiveTimer.Elapsed += OnFestiveTimerElapsed;
        _festiveTimer.Start();

        Log.Information("Started festive avatar timer");
        
        return Task.CompletedTask;
    }

    private static async void OnMoonTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // Check if the current bot is in the guild
        SocketGuild? guild = _discordClient?.CurrentUser.MutualGuilds
            .FirstOrDefault(x => x.Id == KumaDiscordConfig.DiscordMoonGuildId);
        
        if (guild == null) return;

        Log.Information("Checking moon phase");
        
        // Determine what the current image should be
        int phase = Moon.GetMoonPhase(DateTime.UtcNow);
        string phasePath = $"Resources/MoonPhases/Phase{phase}.jpg";
        if (Moon.IsBlueMoon(DateTime.UtcNow)) phasePath = $"Resources/MoonPhases/BlueMoon.jpg";

        // Skip if the image is already set
        const string cacheKey = "discord:moon";
        string? currentPhase = await Cache.GetAsync(cacheKey);
        if (currentPhase == phasePath) return;

        // Update the image 
        Log.Information("Updating moon phase: {CurrentMoonPhase}", phasePath);

        Image phaseImage = new(phasePath);
        await guild.ModifyAsync(
            x => x.Icon = phaseImage, 
            _defaultDiscordRequestOptions);
        await Cache.SetAsync(cacheKey, phasePath);
    }

    private static async void OnMahjongTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // Check if the current bot is in the guild
        SocketGuild? guild = _discordClient?.CurrentUser.MutualGuilds
            .FirstOrDefault(x => x.Id == KumaDiscordConfig.DiscordMahjongGuildId);

        if (guild == null) return;

        // Skip if the image is already set in the last UTC day
        string now = DateTime.UtcNow.ToString("YYYYMMdd");

        const string cacheKey = "discord:mahjong";
        string? lastSet = await Cache.GetAsync(cacheKey);

        if (now == lastSet) return;

        // Get a random tile
        string tile = await Mahjong.GetTile();
        string tilePath = $"Resources/MahjongTiles/{tile}.jpg";

        // Update the image
        Log.Information("Updating mahjong tile: {Tile}", tile);

        Image tileImage = new(tilePath);
        await guild.ModifyAsync(
            x => x.Icon = tileImage,
            _defaultDiscordRequestOptions);
        await Cache.SetAsync(cacheKey, now);
    }

    private static async void OnFestiveTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_discordClient == null) return;
        
        Log.Information("Checking avatar");
        
        // Determine what the current avatar should be
        bool avatarShouldBeFestive = DateTime.UtcNow.Month >= 11;
        string avatar = avatarShouldBeFestive ? "Festive" : "Standard";
        string avatarPath = $"Resources/Avatars/Kuma{avatar}.png";

        // Skip if the correct avatar is already set
        const string cacheKey = "discord:avatar";
        string? currentAvatar = await Cache.GetAsync(cacheKey);
        if (currentAvatar == avatarPath) return;
        
        // Update the avatar
        Log.Information("Updating avatar: {Avatar}", avatarPath);
        
        Image avatarImage = new(avatarPath);
        await _discordClient.CurrentUser.ModifyAsync(
            x => x.Avatar = avatarImage, 
            _defaultDiscordRequestOptions);
        await Cache.SetAsync(cacheKey, avatarPath);
    }

    private static async Task OnDiscordMessageReceived(SocketMessage message)
    {
        // Ignore messages from webhooks and self
        if (message.Author.IsWebhook) return;
        if (message.Author.Id == _discordClient?.CurrentUser.Id) return;

        // Determine if the channel is private or allows NSFW
        ulong channelId;
        bool isPrivate;
        bool isNsfw;
        switch (message.Channel)
        {
            case SocketTextChannel textChannel:
                channelId = textChannel.Id;
                isPrivate = false;
                isNsfw = textChannel.IsNsfw;
                break;
            case SocketDMChannel dmChannel:
                channelId = dmChannel.Id;
                isPrivate = true;
                isNsfw = true;
                break;
            default:
                return;
        }
        
        // Determine requester's user authority
        bool isAdmin = message.Author.Id == KumaDiscordConfig.DiscordAdminId;
        bool isModerator = false;
        if (message.Author is IGuildUser guildUser)
        {
            isModerator = KumaDiscordConfig.DiscordModRoleId != null && 
                          guildUser.RoleIds.Contains(KumaDiscordConfig.DiscordModRoleId.Value);
        }

        UserAuthority authority = UserAuthority.User;
        if (isAdmin) authority = UserAuthority.Administrator;
        else if (isModerator) authority = UserAuthority.Moderator;

        // Send the request
        KumaRequest kumaRequest = new(
            message.Author.Username,
            message.Content,
            SourceSystem.Discord,
            message.Id.ToString(),
            authority,
            channelId.ToString(),
            isPrivate,
            isNsfw);

        await Redis.AddRequestToStreamAsync(kumaRequest);

        if (isAdmin)
        {
            if (message.Content.StartsWith("!makeRoleColorsPost")) await MakeRoleColorsPost(channelId);
        }
    }

    private static async Task OnDiscordReactionAdded(
        Cacheable<IUserMessage, ulong> userMessage,
        Cacheable<IMessageChannel, ulong> messageChannel,
        SocketReaction reaction)
    {
        // Ignore reactions from self
        if (reaction.UserId == _discordClient?.CurrentUser.Id) return;

        IUserMessage? message = await userMessage.GetOrDownloadAsync();
        if (message == null) return;

        // Handle role colors
        if (RoleColors.ContainsValue(reaction.Emote.Name))
            await HandleRoleColorReaction(messageChannel.Value, message, reaction);
    }

    private static async Task HandleRoleColorReaction(IMessageChannel? messageChannel, IUserMessage userMessage, SocketReaction reaction)
    {
        if (messageChannel is not SocketGuildChannel guildChannel) return;

        string cacheKey = $"discord:guild:{guildChannel.Guild.Id}:roles:colors:message_id";
        string? roleColorMessageId = await Cache.GetAsync(cacheKey);

        if (string.IsNullOrEmpty(roleColorMessageId)) return;
        if (userMessage.Id.ToString() != roleColorMessageId) return;

        string desiredRole = RoleColors.FirstOrDefault(x => x.Value == reaction.Emote.Name).Key;
        SocketGuildUser? user = guildChannel.GetUser(reaction.UserId);

        // Add list of colors currently on the user
        List<ulong> rolesToRemove = [];
        foreach (SocketRole role in user.Roles)
        {
            if (!RoleColors.ContainsKey(role.Name)) continue;
            if (role.Name.ToLower() == desiredRole) continue;

            rolesToRemove.Add(role.Id);
        }

        if (desiredRole != "None")
        {
            // Get the role ID for the requested color
            string? roleIdCache =
                await Cache.GetAsync($"discord:guild:{guildChannel.Guild.Id}:roles:colors:{desiredRole.ToLower()}");

            if (string.IsNullOrEmpty(roleIdCache))
            {
                Log.Information("Updating cached role IDs for guild {GuildId}", guildChannel.Guild.Id);

                foreach (SocketRole? role in guildChannel.Guild.Roles ?? [])
                {
                    if (!RoleColors.ContainsKey(role.Name)) continue;

                    string roleCacheKey = $"discord:guild:{guildChannel.Guild.Id}:roles:colors:{role.Name.ToLower()}";
                    await Cache.SetAsync(roleCacheKey, role.Id.ToString());
                }

                roleIdCache =
                    await Cache.GetAsync($"discord:guild:{guildChannel.Guild.Id}:roles:colors:{desiredRole.ToLower()}");
            }

            bool parsedRoleId = ulong.TryParse(roleIdCache, out ulong roleId);
            if (parsedRoleId)
            {
                // Add the role
                await user.AddRoleAsync(roleId, _defaultDiscordRequestOptions);
                rolesToRemove.Remove(roleId);
            }
        }

        // Remove other colors that the user has
        if (rolesToRemove.Count > 0) await user.RemoveRolesAsync(rolesToRemove);

        await userMessage.RemoveReactionAsync(
            reaction.Emote,
            reaction.UserId,
            _defaultDiscordRequestOptions);
    }

    private static async void OnStreamEntryReceived(NameValueEntry entry)
    {
        if (_discordClient == null) return;
        if (entry.Value.IsNullOrEmpty) return;

        KumaResponse? kumaResponse = JsonSerializer.Deserialize<KumaResponse>(entry.Value!);
        if (kumaResponse?.ChannelId == null) return;

        bool parsedChannelId = ulong.TryParse(kumaResponse.ChannelId, out ulong channelId);
        if (!parsedChannelId) return;

        if (await _discordClient.GetChannelAsync(channelId, _defaultDiscordRequestOptions)
            is not ISocketMessageChannel channel) return;

        try
        {
            // Send an embedded image if one is attached
            if (kumaResponse.Media != null)
            {
                EmbedBuilder embed = new()
                {
                    Color = new Color(0x00b6b6),
                    Title = kumaResponse.Media.Referrer,
                    Url = kumaResponse.Media.Source,
                    Description = kumaResponse.Media.Description,
                    ImageUrl = kumaResponse.Media.Url,
                    Timestamp = DateTime.UtcNow
                };

                await channel.SendMessageAsync(
                    text: kumaResponse.Message,
                    embed: embed.Build(),
                    options: _defaultDiscordRequestOptions);

                if (!SupportedVideoFileTypes.Any(x => kumaResponse.Media.Url.EndsWith(x))) return;

                string encodedVideoUrl = HttpUtility.UrlEncode(kumaResponse.Media.Url);
                string encodedPreviewUrl = HttpUtility.UrlEncode(kumaResponse.Media.Preview);

                // TODO: should figure out how AV1 stuff works and implement it myself
                await channel.SendMessageAsync(
                    text: $"[Video](https://autocompressor.net/av1?v={encodedVideoUrl}&i={encodedPreviewUrl})",
                    options: _defaultDiscordRequestOptions);
            }
            // Send a standard message
            else if (!string.IsNullOrEmpty(kumaResponse.Message))
            {
                await channel.SendMessageAsync(
                    text: kumaResponse.Message,
                    flags: MessageFlags.SuppressEmbeds,
                    options: _defaultDiscordRequestOptions);
            }
        }
        catch (HttpException ex)
        {
            Log.Warning(ex, "An exception was thrown while sending a message to Discord, likely permissions issues");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An exception was thrown while sending a message to Discord");
        }
    }

    private static async Task MakeRoleColorsPost(ulong channelId)
    {
        if (_discordClient == null) return;

        if (await _discordClient.GetChannelAsync(channelId, _defaultDiscordRequestOptions)
            is not ISocketMessageChannel channel) return;
        if (channel is not SocketGuildChannel guildChannel) return;

        // Create the message
        List<string> description = ["React to color your username.\n"];
        foreach (string color in RoleColors.Keys) description.Add($"{RoleColors[color]} - {color}");

        EmbedBuilder embed = new()
        {
            Color = new Color(0x00b6b6),
            Title = "Color Roles",
            Description = string.Join("\n", description)
        };

        RestUserMessage? message = await channel.SendMessageAsync(
            embed: embed.Build(),
            options: _defaultDiscordRequestOptions);

        // Create the reactions
        foreach (string emoji in RoleColors.Values) await message.AddReactionAsync(new Emoji(emoji));

        string cacheKey = $"discord:guild:{guildChannel.Guild.Id}:roles:colors:message_id";
        await Cache.SetAsync(cacheKey, message.Id.ToString());

        Log.Information("Role colors post made for guild ID {GuildId} with message ID {MessageId}",
            guildChannel.Guild.Id, message.Id);
    }
}