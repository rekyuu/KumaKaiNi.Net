using KumaKaiNi.Core;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;
using Serilog;
using StackExchange.Redis;

namespace KumaKaiNi.Client.YouTube;

internal static class Program
{
    private static RedisStreamConsumer? _streamConsumer;
    private static CancellationTokenSource? _cts;

    private static async Task Main(string[] args)
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

            _cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                _cts.Cancel();
                eventArgs.Cancel = true;
            };

            _streamConsumer = new RedisStreamConsumer(
                Redis.GetStreamNameForSourceSystem(SourceSystem.YouTube),
                cancellationToken: _cts.Token);

            _streamConsumer.StreamEntryReceived += OnStreamEntryReceived;
            await _streamConsumer.StartAsync();

            // Start the client around here

            await Redis.SendDeploymentNotificationToAdmin();

            await Task.Delay(-1, _cts.Token);
        }
        catch (TaskCanceledException)
        {
            // Stop the client here
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

    private static void OnStreamEntryReceived(NameValueEntry entry)
    {
        throw new NotImplementedException();
    }

    private static async Task GetLiveVideos()
    {
        throw new NotImplementedException();
    }
}