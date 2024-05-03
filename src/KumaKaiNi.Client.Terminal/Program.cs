using System.Text.Json;
using KumaKaiNi.Core;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;
using Serilog;
using StackExchange.Redis;

namespace KumaKaiNi.Client.Terminal;

internal static class Program
{
    private static string ConsumerStreamName => Redis.GetStreamNameForSourceSystem(SourceSystem.Terminal);
    
    private static KumaClient? _kuma;
    private static RedisStreamConsumer? _streamConsumer;
    private static CancellationTokenSource? _cts;

    private static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(KumaConfig.GetLogLevel())
            .WriteTo.Console()
            .CreateLogger();

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
                ConsumerStreamName,
                cancellationToken: _cts.Token);

            _streamConsumer.StreamEntriesReceived += OnStreamEntriesReceived;
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
                channelId: 5862,
                channelIsPrivate: true,
                channelIsNsfw: true);

            _ = useRedisStreams ? Redis.AddRequestToStream(kumaRequest) : _kuma?.ProcessRequest(kumaRequest);
        }

        await Log.CloseAndFlushAsync();
        
        return 0;
    }

    private static void OnKumaResponse(KumaResponse kumaResponse)
    {
        Log.Verbose("Got response: {Response}", kumaResponse);
    }

    private static void OnStreamEntriesReceived(StreamEntry[] streamEntries)
    {
        foreach (StreamEntry streamEntry in streamEntries)
        {
            foreach (NameValueEntry entry in streamEntry.Values)
            {
                if (entry.Value.IsNullOrEmpty) continue;
        
                KumaResponse? response = JsonSerializer.Deserialize<KumaResponse>(entry.Value!);
                if (response == null) continue;
                
                OnKumaResponse(response);
            }
        }
    }
}