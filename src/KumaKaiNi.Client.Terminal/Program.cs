using System.Text.Json;
using KumaKaiNi.Core;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;
using Serilog;
using StackExchange.Redis;

namespace KumaKaiNi.Client.Terminal;

internal static class Program
{
    private static UserAuthority _authority = UserAuthority.Administrator;
    private static bool _isPrivate = true;
    private static bool _isNsfw = true;

    private static KumaClient? _kuma;
    private static RedisStreamConsumer? _streamConsumer;
    private static CancellationTokenSource? _cts;

    private static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(KumaRuntimeConfig.GetLogLevel())
            .WriteTo.Console()
            .CreateLogger();
        
        Log.Information("Starting {ApplicationName} {ApplicationVersion} on {MachineName}", 
            KumaRuntimeConfig.ApplicationName, 
            KumaRuntimeConfig.ApplicationVersion, 
            Environment.MachineName);

        bool useRedisStreams = args.Contains("--streams");
        
        _cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            _cts.Cancel();
            eventArgs.Cancel = true;
            _streamConsumer?.Stop();

            Environment.Exit(0);
        };

        if (useRedisStreams)
        {
            _streamConsumer = new RedisStreamConsumer(
                Redis.GetStreamNameForSourceSystem(SourceSystem.Terminal),
                cancellationToken: _cts.Token);

            _streamConsumer.StreamEntryReceived += OnStreamEntryReceived;
            await _streamConsumer.StartAsync();
        }
        else
        {
            _kuma = new KumaClient();
            _kuma.Responded += OnKumaResponse;
        }
        
        while (true)
        {
            if (_cts.IsCancellationRequested) break;

            string? message = Console.ReadLine();

            if (string.IsNullOrEmpty(message)) continue;
            switch (message)
            {
                case "exit":
                    Environment.Exit(0);
                    break;
                case "makeAdmin":
                    _authority = UserAuthority.Administrator;
                    Log.Information("UserAuthority set to Administrator");
                    continue;
                case "makeMod":
                    _authority = UserAuthority.Moderator;
                    Log.Information("UserAuthority set to Moderator");
                    continue;
                case "makeUser":
                    _authority = UserAuthority.User;
                    Log.Information("UserAuthority set to User");
                    continue;
                case "makePrivate":
                    _isPrivate = true;
                    Log.Information("Chat visibility set to private");
                    continue;
                case "makePublic":
                    _isPrivate = false;
                    Log.Information("Chat visibility set to public");
                    continue;
                case "makeNsfw":
                    _isNsfw = true;
                    Log.Information("Chat set to NSFW");
                    continue;
                case "makeSfw":
                    _isNsfw = false;
                    Log.Information("Chat set to SFW");
                    continue;
            }

            KumaRequest kumaRequest = new(
                "rekyuu",
                message,
                SourceSystem.Terminal,
                userAuthority: _authority,
                channelId: "5862",
                channelIsPrivate: _isPrivate,
                channelIsNsfw: _isNsfw);

            _ = useRedisStreams ? Redis.AddRequestToStreamAsync(kumaRequest) : _kuma?.ProcessRequest(kumaRequest);
        }

        await Log.CloseAndFlushAsync();
        
        return 0;
    }

    private static void OnKumaResponse(KumaResponse kumaResponse)
    {
        Log.Information("Got response: {Response}", kumaResponse);
    }

    private static void OnStreamEntryReceived(NameValueEntry entry)
    {
        if (entry.Value.IsNullOrEmpty) return;
        
        KumaResponse? response = JsonSerializer.Deserialize<KumaResponse>(entry.Value!);
        if (response == null) return;
                
        OnKumaResponse(response);
    }
}