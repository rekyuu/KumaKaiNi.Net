using System.Text.Json;
using KumaKaiNi.Core;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;
using Serilog;
using StackExchange.Redis;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using OnConnectedEventArgs = TwitchLib.Client.Events.OnConnectedEventArgs;

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
            WebSocketClient wsClient = new();
            _twitchClient = new TwitchClient(wsClient);
            _twitchClient.Initialize(_twitchCredentials, KumaTwitchConfig.TwitchChannel);

            _twitchClient.OnConnected += OnTwitchConnected;
            _twitchClient.OnJoinedChannel += OnTwitchJoinedChannel;
            _twitchClient.OnLeftChannel += OnTwitchLeftChannel;
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

            await _twitchClient.ConnectAsync();
            await Redis.SendDeploymentNotificationToAdmin();

            await Task.Delay(-1, _cts.Token);
        }
        catch (TaskCanceledException)
        {
            await LeaveAndDisconnectTwitchClient();
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

    private static Task OnTwitchConnected(object? sender, OnConnectedEventArgs e)
    {
        Log.Information("Connected as {Username}", e.BotUsername);
        return Task.CompletedTask;
    }

    private static Task OnTwitchJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        Log.Information("Joined {Channel}", e.Channel);
        return Task.CompletedTask;
    }

    private static Task OnTwitchLeftChannel(object? sender, OnLeftChannelArgs e)
    {
        Log.Information("Left {Channel}", e.Channel);
        return Task.CompletedTask;
    }

    private static async Task OnTwitchMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        UserAuthority authority = UserAuthority.User;
        if (e.ChatMessage.IsBroadcaster) authority = UserAuthority.Administrator;
        if (e.ChatMessage.UserType == UserType.Moderator) authority = UserAuthority.Moderator;

        KumaRequest kumaRequest = new(
            e.ChatMessage.Username,
            e.ChatMessage.Message,
            SourceSystem.Twitch,
            e.ChatMessage.Id,
            authority,
            e.ChatMessage.Channel);

        await Redis.AddRequestToStreamAsync(kumaRequest);
    }

    private static Task OnTwitchWhisperReceived(object? sender, OnWhisperReceivedArgs e)
    {
        // Ignore for now
        return Task.CompletedTask;
    }

    private static Task OnTwitchError(object? sender, OnErrorEventArgs e)
    {
        Log.Error(e.Exception, "A Twitch error occurred");
        return Task.CompletedTask;
    }

    private static Task OnTwitchIncorrectLogin(object? sender, OnIncorrectLoginArgs e)
    {
        Log.Error(e.Exception, "Incorrect Twitch login");
        return Task.CompletedTask;
    }

    private static Task OnTwitchConnectionError(object? sender, OnConnectionErrorArgs e)
    {
        Log.Error("Twitch connection error: {Message}", e.Error.Message);
        return Task.CompletedTask;
    }

    private static Task OnTwitchDisconnected(object? sender, OnDisconnectedArgs e)
    {
        // TODO: might need to add reconnect logic here but idk what that looks like yet
        Log.Error("Twitch disconnected");
        return Task.CompletedTask;
    }

    private static Task OnTwitchReconnected(object? sender, OnConnectedEventArgs e)
    {
        Log.Information("Twitch reconnected");
        return Task.CompletedTask;
    }

    private static Task OnTwitchFailureToReceiveJoinConfirmation(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
    {
        Log.Error("Failed to receive join confirmation for {Channel}: {Details}",
            e.Exception.Channel, e.Exception.Details);
        return Task.CompletedTask;
    }

    private static Task OnTwitchNoPermissionError(object? sender, NoticeEventArgs e)
    {
        Log.Error("Twitch no permissions error has occurred");
        return Task.CompletedTask;
    }

    private static Task OnTwitchRateLimit(object? sender, NoticeEventArgs e)
    {
        Log.Error("Twitch rate limit has been enforced");
        return Task.CompletedTask;
    }

    private static async void OnStreamEntryReceived(NameValueEntry entry)
    {
        if (_twitchClient == null) return;
        await ValidateTwitchClient();

        if (entry.Value.IsNullOrEmpty) return;

        KumaResponse? kumaResponse = JsonSerializer.Deserialize<KumaResponse>(entry.Value!);
        if (kumaResponse?.ChannelId == null) return;
        if (string.IsNullOrEmpty(kumaResponse.Message)) return;

        await _twitchClient.SendMessageAsync(kumaResponse.ChannelId, kumaResponse.Message);
    }

    private static async Task ValidateTwitchClient()
    {
        if (_twitchClient == null) return;

        if (!_twitchClient.IsInitialized)
        {
            if (_twitchCredentials == null) throw new Exception("Twitch credentials is null");

            Log.Warning("Attempting to reinitialize the Twitch client");
            _twitchClient.Initialize(_twitchCredentials, KumaTwitchConfig.TwitchChannel);

            if (!_twitchClient.IsInitialized) throw new Exception("The Twitch client is not initialized");
        }

        if (!_twitchClient.IsConnected)
        {
            Log.Warning("Attempting to reconnect the Twitch client");
            await _twitchClient.ReconnectAsync();

            if (!_twitchClient.IsConnected) throw new Exception("The Twitch client is not connected");
        }

        if (_twitchClient.JoinedChannels.Count == 0)
        {
            if (string.IsNullOrEmpty(KumaTwitchConfig.TwitchChannel)) throw new Exception("Twitch channel is null");

            Log.Warning("Attempting to rejoin the Twitch client to {Channel}", KumaTwitchConfig.TwitchChannel);
            await _twitchClient.JoinChannelAsync(KumaTwitchConfig.TwitchChannel);

            if (_twitchClient.JoinedChannels.Count == 0)
                throw new Exception($"The Twitch client is not joined to {KumaTwitchConfig.TwitchChannel}");
        }
    }

    private static async Task LeaveAndDisconnectTwitchClient()
    {
        if (_twitchClient == null) return;

        // Always leave the default channel
        if (!string.IsNullOrEmpty(KumaTwitchConfig.TwitchChannel))
            await _twitchClient.LeaveChannelAsync(KumaTwitchConfig.TwitchChannel);

        // Leave other channels if any
        foreach (JoinedChannel channel in _twitchClient.JoinedChannels)
        {
            await _twitchClient.LeaveChannelAsync(channel);
        }

        // Disconnect
        await _twitchClient.DisconnectAsync();
    }
}