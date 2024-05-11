using System.Text.Json;
using KumaKaiNi.Core;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;
using Serilog;
using StackExchange.Redis;

namespace KumaKaiNi.Client.Terminal;

internal static class Program
{
    private static KumaClient? _kuma;
    private static RedisStreamConsumer? _streamConsumer;
    private static CancellationTokenSource? _cts;

    private static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(KumaConfig.GetLogLevel())
            .WriteTo.Console()
            .CreateLogger();
        
        Log.Information("Starting {ApplicationName} {ApplicationVersion} on {MachineName}", 
            KumaConfig.ApplicationName, 
            KumaConfig.ApplicationVersion, 
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
            if (message == "exit") Environment.Exit(0);

            KumaRequest kumaRequest = new(
                "rekyuu",
                message,
                SourceSystem.Terminal,
                userAuthority: UserAuthority.User,
                channelId: "5862",
                channelIsPrivate: true,
                channelIsNsfw: true);

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