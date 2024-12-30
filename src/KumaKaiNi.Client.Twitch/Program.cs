using System.Text.Json;
using KumaKaiNi.Core;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;
using Serilog;
using StackExchange.Redis;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace KumaKaiNi.Client.Twitch;

internal static class Program
{
    private static TwitchClient? _twitchClient;
    private static RedisStreamConsumer? _streamConsumer;
    private static CancellationTokenSource? _cts;

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

            if (string.IsNullOrEmpty(KumaTwitchConfig.TwitchChannel))
            {
                Log.Fatal("TWITCH_CHANNEL environment variable must be set, exiting");
                Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(KumaTwitchConfig.TwitchUsername))
            {
                Log.Fatal("TWITCH_USERNAME environment variable must be set, exiting");
                Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(KumaTwitchConfig.TwitchAccessToken))
            {
                Log.Fatal("TWITCH_ACCESS_TOKEN environment variable must be set, exiting");
                Environment.Exit(1);
            }

            _cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                _cts.Cancel();
                eventArgs.Cancel = true;
            };

            _streamConsumer = new RedisStreamConsumer(
                Redis.GetStreamNameForSourceSystem(SourceSystem.Twitch),
                cancellationToken: _cts.Token);

            _streamConsumer.StreamEntryReceived += OnStreamEntryReceived;
            await _streamConsumer.StartAsync();

            // Had to be obtained via implicit grant flow with chat:read and chat:edit permissions.
            // https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#implicit-grant-flow
            // https://dev.twitch.tv/docs/authentication/scopes/#irc-chat-scopes
            ConnectionCredentials credentials = new(KumaTwitchConfig.TwitchUsername, KumaTwitchConfig.TwitchAccessToken);
            ClientOptions clientOptions = new()
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient wsClient = new(clientOptions);
            _twitchClient = new TwitchClient(wsClient);
            _twitchClient.Initialize(credentials, KumaTwitchConfig.TwitchChannel);

            _twitchClient.OnConnected += OnTwitchConnected;
            _twitchClient.OnLog += OnTwitchLog;
            _twitchClient.OnJoinedChannel += OnTwitchJoinedChannel;
            _twitchClient.OnMessageReceived += OnTwitchMessageReceived;
            _twitchClient.OnWhisperReceived += OnTwitchWhisperReceived;

            _twitchClient.Connect();
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

    private static void OnTwitchConnected(object? sender, OnConnectedArgs e)
    {
        Log.Information("Connected to {Channel}", e.AutoJoinChannel);
    }

    private static void OnTwitchLog(object? sender, OnLogArgs e)
    {
        Log.Information("[Twitch] {Data}", e.Data);
    }

    private static void OnTwitchJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        Log.Information("Joined {Channel}", e.Channel);
    }

    private static async void OnTwitchMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        UserAuthority authority = UserAuthority.User;
        if (e.ChatMessage.IsBroadcaster) authority = UserAuthority.Administrator;
        if (e.ChatMessage.IsModerator) authority = UserAuthority.Moderator;

        KumaRequest kumaRequest = new(
            e.ChatMessage.Username,
            e.ChatMessage.Message,
            SourceSystem.Twitch,
            e.ChatMessage.Id,
            authority,
            e.ChatMessage.Channel);

        await Redis.AddRequestToStreamAsync(kumaRequest);
    }

    private static void OnTwitchWhisperReceived(object? sender, OnWhisperReceivedArgs e)
    {
        // Ignore for now
    }

    private static void OnStreamEntryReceived(NameValueEntry entry)
    {
        if (_twitchClient == null) return;
        if (entry.Value.IsNullOrEmpty) return;

        KumaResponse? kumaResponse = JsonSerializer.Deserialize<KumaResponse>(entry.Value!);
        if (kumaResponse?.ChannelId == null) return;

        _twitchClient.SendMessage(kumaResponse.ChannelId, kumaResponse.Message);
    }
}