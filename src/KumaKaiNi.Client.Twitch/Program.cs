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
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

namespace KumaKaiNi.Client.Twitch;

internal static class Program
{
    private static ConnectionCredentials? _twitchCredentials;
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
            _twitchCredentials = new ConnectionCredentials(KumaTwitchConfig.TwitchUsername, KumaTwitchConfig.TwitchAccessToken);
            ClientOptions clientOptions = new()
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient wsClient = new(clientOptions);
            _twitchClient = new TwitchClient(wsClient)
            {
                AutoReListenOnException = true
            };
            _twitchClient.Initialize(_twitchCredentials, KumaTwitchConfig.TwitchChannel);

            _twitchClient.OnConnected += OnTwitchConnected;
            _twitchClient.OnLog += OnTwitchLog;
            _twitchClient.OnJoinedChannel += OnTwitchJoinedChannel;
            _twitchClient.OnMessageReceived += OnTwitchMessageReceived;
            _twitchClient.OnWhisperReceived += OnTwitchWhisperReceived;

            _twitchClient.OnError += OnTwitchError;
            _twitchClient.OnIncorrectLogin += OnTwitchIncorrectLogin;
            _twitchClient.OnConnectionError += OnTwitchConnectionError;
            _twitchClient.OnDisconnected += OnTwitchDisconnected;
            _twitchClient.OnReconnected += OnTwitchReconnected;
            _twitchClient.OnFailureToReceiveJoinConfirmation += OnTwitchFailureToReceiveJoinConfirmation;
            _twitchClient.OnNoPermissionError += OnTwitchNoPermissionError;
            _twitchClient.OnRateLimit += OnTwitchRateLimit;

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

    private static void OnTwitchJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        Log.Information("Joined {Channel}", e.Channel);
    }

    private static void OnTwitchError(object? sender, OnErrorEventArgs e)
    {
        Log.Error(e.Exception, "A Twitch error occurred");
    }

    private static void OnTwitchIncorrectLogin(object? sender, OnIncorrectLoginArgs e)
    {
        Log.Error(e.Exception, "Incorrect Twitch login");
    }

    private static void OnTwitchConnectionError(object? sender, OnConnectionErrorArgs e)
    {
        Log.Error("Twitch connection error: {Message}", e.Error.Message);
    }

    private static void OnTwitchDisconnected(object? sender, OnDisconnectedEventArgs e)
    {
        Log.Error("Twitch disconnected");
        // TODO: might need to add reconnect logic here but idk what that looks like yet
    }

    private static void OnTwitchReconnected(object? sender, OnReconnectedEventArgs e)
    {
        Log.Information("Twitch reconnected");
    }

    private static void OnTwitchFailureToReceiveJoinConfirmation(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
    {
        Log.Error("Failed to receive join confirmation for {Channel}: {Details}",
            e.Exception.Channel, e.Exception.Details);
    }

    private static void OnTwitchNoPermissionError(object? sender, EventArgs e)
    {
        Log.Error("Twitch no permissions error has occurred");
    }

    private static void OnTwitchRateLimit(object? sender, OnRateLimitArgs e)
    {
        Log.Error("Twitch rate limit has been enforced");
    }

    private static void OnStreamEntryReceived(NameValueEntry entry)
    {
        if (_twitchClient == null) return;
        ValidateTwitchClient();

        if (entry.Value.IsNullOrEmpty) return;

        KumaResponse? kumaResponse = JsonSerializer.Deserialize<KumaResponse>(entry.Value!);
        if (kumaResponse?.ChannelId == null) return;

        _twitchClient.SendMessage(kumaResponse.ChannelId, kumaResponse.Message);
    }

    private static void ValidateTwitchClient()
    {
        if (_twitchClient == null) return;

        if (!_twitchClient.IsInitialized)
        {
            Log.Warning("Attempting to reinitialize the Twitch client");
            _twitchClient.Initialize(_twitchCredentials, KumaTwitchConfig.TwitchChannel);

            if (!_twitchClient.IsInitialized) throw new Exception("The Twitch client is not initialized");
        }

        if (!_twitchClient.IsConnected)
        {
            Log.Warning("Attempting to reconnect the Twitch client");
            _twitchClient.Reconnect();

            if (!_twitchClient.IsConnected) throw new Exception("The Twitch client is not connected");
        }

        if (_twitchClient.JoinedChannels.Count == 0)
        {
            Log.Warning("Attempting to rejoin the Twitch client to {Channel}", KumaTwitchConfig.TwitchChannel);
            _twitchClient.JoinChannel(KumaTwitchConfig.TwitchChannel);

            if (_twitchClient.JoinedChannels.Count == 0)
                throw new Exception($"The Twitch client is not joined to {KumaTwitchConfig.TwitchChannel}");
        }
    }
}