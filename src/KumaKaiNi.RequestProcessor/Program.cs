using System.Text.Json;
using KumaKaiNi.Core;
using KumaKaiNi.Core.Database;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

namespace KumaKaiNi.RequestProcessor;

public static class Program
{
    private static KumaClient? _kuma;
    private static RedisStreamConsumer? _streamConsumer;
    private static CancellationTokenSource? _cts;
    
    private static async Task Main()
    {
        try
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(KumaConfig.GetLogLevel())
                .WriteTo.Console()
                .CreateLogger();
            
            _cts = new CancellationTokenSource();

            await MigrateDatabase(_cts.Token);
        
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                _cts.Cancel();
                eventArgs.Cancel = true;
                _streamConsumer?.Stop();

                Environment.Exit(0);
            };
        
            _kuma = new KumaClient();
            _kuma.Responded += OnKumaResponse;
        
            _streamConsumer = new RedisStreamConsumer(
                Redis.KumaConsumerStreamName,
                cancellationToken: _cts.Token);

            _streamConsumer.StreamEntryReceived += OnStreamEntryReceived;
            await _streamConsumer.StartAsync();
        
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

    private static async Task MigrateDatabase(CancellationToken cancellationToken)
    {
        await using KumaKaiNiDbContext db = new();
            
        IEnumerable<string> migrations = await db.Database.GetPendingMigrationsAsync(cancellationToken);
        if (!migrations.Any()) return;
        
        Log.Information("Running database migrations");
        await db.Database.MigrateAsync(cancellationToken);
    }

    private static async void OnKumaResponse(KumaResponse kumaResponse)
    {
        string destinationStream = Redis.GetStreamNameForSourceSystem(kumaResponse.SourceSystem);
        string serializedRequest = JsonSerializer.Serialize(kumaResponse);
                
        IDatabase? db = Redis.Database;
        if (db == null) return;

        await db.StreamAddAsync(
            destinationStream,
            [new NameValueEntry("response", serializedRequest)],
            maxLength: Redis.StreamMaxLength,
            useApproximateMaxLength: true);
    }

    private static void OnStreamEntryReceived(NameValueEntry entry)
    {
        if (entry.Value.IsNullOrEmpty) return;
        
        KumaRequest? response = JsonSerializer.Deserialize<KumaRequest>(entry.Value!);
        if (response == null) return;
                
        _ = _kuma?.ProcessRequest(response);
    }
}