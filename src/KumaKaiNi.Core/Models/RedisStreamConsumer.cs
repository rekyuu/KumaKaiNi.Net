using System.Timers;
using KumaKaiNi.Core.Utility;
using Serilog;
using StackExchange.Redis;
using Timer = System.Timers.Timer;

namespace KumaKaiNi.Core.Models;

public class RedisStreamConsumer
{
    /// <summary>
    /// Delegate for when stream entries are received
    /// </summary>
    public delegate void StreamEntryReceivedEventHandler(NameValueEntry entry);
    
    /// <summary>
    /// Event handler for stream entries when they're received
    /// </summary>
    public event StreamEntryReceivedEventHandler? StreamEntryReceived;
    
    private const string GroupName = "consumers";
    
    private readonly string _streamName;
    private readonly string _consumerName = Guid.NewGuid().ToString();
    
    private readonly Timer _streamRangeTimer;
    private readonly Timer _responseConsumerTimer;
    
    private readonly CancellationToken? _cancellationToken;

    public RedisStreamConsumer(string streamName, double streamReadInterval = 1000, CancellationToken? cancellationToken = null)
    {
        _streamName = streamName;
        _cancellationToken = cancellationToken;

        _streamRangeTimer = new Timer(streamReadInterval);
        _streamRangeTimer.Elapsed += OnStreamRangeElapsed;
        _streamRangeTimer.AutoReset = false;
        
        _responseConsumerTimer = new Timer(streamReadInterval);
        _responseConsumerTimer.Elapsed += OnResponseConsumeElapsed;
        _responseConsumerTimer.AutoReset = false;
    }

    public async Task StartAsync()
    {
        IDatabase? db = Redis.Database;
        if (db == null) return;
        
        bool streamExists = await db.KeyExistsAsync(_streamName);
        bool streamGroupExists = false;
        if (streamExists)
        {
            StreamGroupInfo[] groupInfo = await db.StreamGroupInfoAsync(_streamName);
            
            if (groupInfo.Length == 0) streamGroupExists = false;
            else streamGroupExists = groupInfo.All(x => x.Name == GroupName);
        }

        if (!streamExists || !streamGroupExists)
        {
            Log.Information("Creating consumer group: {StreamName} - {GroupName}",
                _streamName,
                GroupName);
                
            await db.StreamCreateConsumerGroupAsync(
                _streamName,
                GroupName,
                "0-0");
        }
        
        Log.Information("Starting stream consumer: {StreamName} - {GroupName}",
            _streamName,
            GroupName);
        
        _streamRangeTimer.Start();
        _responseConsumerTimer.Start();
    }

    public void Stop()
    {
        _streamRangeTimer.Stop();
        _responseConsumerTimer.Stop();
    }

    private async void OnStreamRangeElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_cancellationToken?.IsCancellationRequested == true) return;

        try
        {
            IDatabase? db = Redis.Database;
            if (db == null) return;

            // Gets the most recent element
            // https://redis.io/learn/develop/dotnet/streams/stream-basics#spin-up-most-recent-element-task
            await db.StreamRangeAsync(
                _streamName,
                "-",
                "+",
                1,
                Order.Descending);
        }
        catch (Exception ex)
        {
            await Logging.LogExceptionToDatabaseAsync(ex, "An exception was thrown while processing stream range");
        }
        finally
        {
            _streamRangeTimer.Start();
        }
    }

    private async void OnResponseConsumeElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_cancellationToken?.IsCancellationRequested == true) return;

        try
        {
            IDatabase? db = Redis.Database;
            if (db == null) return;

            // Pull all messages that came in the last cadence
            StreamEntry[] streamEntries = await db.StreamReadGroupAsync(
                _streamName,
                GroupName,
                _consumerName,
                ">");
        
            if (streamEntries.Length == 0) return;

            foreach (StreamEntry streamEntry in streamEntries)
            {
                foreach (NameValueEntry entry in streamEntry.Values)
                {
                    StreamEntryReceived?.Invoke(entry);
                }
                
                await db.StreamAcknowledgeAsync(
                    _streamName, 
                    GroupName, 
                    streamEntry.Id);
            }
            
            Log.Verbose("Consumed {Count} stream records for stream {Stream}", 
                streamEntries.Length,
                _streamName);
        }
        catch (Exception ex)
        {
            await Logging.LogExceptionToDatabaseAsync(ex, "An exception was thrown while processing stream entries");
        }
        finally
        {
            _responseConsumerTimer.Start();
        }
    }
}