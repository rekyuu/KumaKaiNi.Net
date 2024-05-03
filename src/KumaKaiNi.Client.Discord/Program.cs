using System.Text.Json;
using System.Timers;
using Discord;
using Discord.WebSocket;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;
using Serilog;
using StackExchange.Redis;
using Timer = System.Timers.Timer;

namespace KumaKaiNi.Discord;

internal static class Program
{
    private static string ConsumerStreamName => Redis.GetStreamNameForSourceSystem(SourceSystem.Discord);
    
    private static RedisStreamConsumer? _streamConsumer;
    private static DiscordSocketClient? _discordClient;
    private static CancellationTokenSource? _cts;
    private static RequestOptions? _defaultDiscordRequestOptions;

    private static Timer? _moonTimer;
    private static Timer? _festiveTimer;
    
    private static async Task Main()
    {
        try
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

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
                ConsumerStreamName,
                cancellationToken: _cts.Token);

            _streamConsumer.StreamEntriesReceived += OnStreamEntriesReceived;
            await _streamConsumer.StartAsync();

            DiscordSocketConfig discordConfig = new()
            {
                GatewayIntents = GatewayIntents.MessageContent |
                                 GatewayIntents.Guilds |
                                 GatewayIntents.GuildMessages
            };

            _discordClient = new DiscordSocketClient(discordConfig);
            _defaultDiscordRequestOptions = new RequestOptions { CancelToken = _cts.Token };
            _discordClient.Log += OnDiscordLog;
            _discordClient.Ready += OnDiscordReady;
            _discordClient.MessageReceived += OnDiscordMessageReceived;
            
            await _discordClient.LoginAsync(TokenType.Bot, KumaDiscordConfig.DiscordToken);
            await _discordClient.StartAsync();

            Log.Information("Listening for updates");
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
    }

    private static Task OnDiscordLog(LogMessage message)
    {
        if (message.Exception != null) Log.Error("[Discord.Net] {Message}", message);
        else Log.Information("[Discord.Net] {Message}", message);
        
        return Task.CompletedTask;
    }

    private static Task OnDiscordReady()
    {
        Log.Information("[Discord.Net] Discord is ready");

        _moonTimer = new Timer(60 * 60 * 1000);
        _moonTimer.Elapsed += OnMoonTimerElapsed;
        _moonTimer.Start();
        
        Log.Information("[KumaKaiNi.Client.Discord] Started moon phase timer");

        _festiveTimer = new Timer(24 * 60 * 60 * 1000);
        _festiveTimer.Elapsed += OnFestiveTimerElapsed;
        _festiveTimer.Start();
        
        Log.Information("[KumaKaiNi.Client.Discord] Started festive avatar timer");
        
        return Task.CompletedTask;
    }

    private static async void OnMoonTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // Check if the current bot is in the guild
        SocketGuild? guild = _discordClient?.CurrentUser.MutualGuilds
            .FirstOrDefault(x => x.Id == KumaDiscordConfig.DiscordMoonGuildId);
        
        if (guild == null) return;

        Log.Information("[KumaKaiNi.Client.Discord] Checking moon phase");
        
        // Determine what the current image should be
        int phase = Moon.GetMoonPhase(DateTime.UtcNow);
        string phasePath = $"Resources/MoonPhases/Phase{phase}.jpg";

        // Skip if the image is already set
        const string cacheKey = "discord:moon";
        string? currentPhase = await Cache.GetAsync(cacheKey);
        if (currentPhase == phasePath) return;

        // Update the image 
        Log.Information("[KumaKaiNi.Client.Discord] Updating moon phase: {CurrentMoonPhase}", phasePath);

        Image phaseImage = new(phasePath);
        await guild.ModifyAsync(
            x => x.Icon = phaseImage, 
            _defaultDiscordRequestOptions);
        await Cache.SetAsync(cacheKey, phasePath);
    }

    private static async void OnFestiveTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_discordClient == null) return;
        
        Log.Information("[KumaKaiNi.Client.Discord] Checking avatar");
        
        // Determine what the current avatar should be
        bool avatarShouldBeFestive = DateTime.UtcNow.Month >= 11;
        string avatar = avatarShouldBeFestive ? "Festive" : "Standard";
        string avatarPath = $"Resources/Avatars/Kuma{avatar}.png";

        // Skip if the correct avatar is already set
        const string cacheKey = "discord:avatar";
        string? currentAvatar = await Cache.GetAsync(cacheKey);
        if (currentAvatar == avatarPath) return;
        
        // Update the avatar
        Log.Information("[KumaKaiNi.Client.Discord] Updating avatar: {Avatar}", avatarPath);
        
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
            (long)message.Id, // Casting ulong as long might cause issues at some point
            authority,
            (long)channelId,
            isPrivate,
            isNsfw);

        await Redis.AddRequestToStream(kumaRequest);
    }

    private static async void OnStreamEntriesReceived(StreamEntry[] streamEntries)
    {
        if (_discordClient == null) return;
        
        foreach (StreamEntry streamEntry in streamEntries)
        {
            foreach (NameValueEntry entry in streamEntry.Values)
            {
                if (entry.Value.IsNullOrEmpty) continue;
        
                KumaResponse? kumaResponse = JsonSerializer.Deserialize<KumaResponse>(entry.Value!);
                if (kumaResponse == null) continue;
                
                if (kumaResponse.ChannelId == null) return;
        
                if (await _discordClient.GetChannelAsync((ulong)kumaResponse.ChannelId, _defaultDiscordRequestOptions) 
                    is not ISocketMessageChannel channel) return;

                // Send an embedded image if one is attached
                if (kumaResponse.Image != null)
                {
                    EmbedBuilder embed = new()
                    {
                        Color = new Color(0x00b6b6),
                        Title = kumaResponse.Image.Referrer,
                        Url = kumaResponse.Image.Source,
                        Description = kumaResponse.Image.Description,
                        ImageUrl = kumaResponse.Image.Url,
                        Timestamp = DateTime.UtcNow
                    };

                    await channel.SendMessageAsync(
                        text: kumaResponse.Message, 
                        embed: embed.Build(), 
                        options: _defaultDiscordRequestOptions);
                }
                // Send a standard message
                else if (!string.IsNullOrEmpty(kumaResponse.Message))
                {
                    await channel.SendMessageAsync(
                        text: kumaResponse.Message, 
                        options: _defaultDiscordRequestOptions);
                }
            }
        }
    }
}