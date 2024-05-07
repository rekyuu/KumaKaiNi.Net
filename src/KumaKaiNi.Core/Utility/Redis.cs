using System.Net;
using System.Text.Json;
using KumaKaiNi.Core.Models;
using Medallion.Threading.Redis;
using Serilog;
using StackExchange.Redis;

namespace KumaKaiNi.Core.Utility;

public static class Redis
{
    public static bool IsConnected => RedisConn is { IsConnected: true };

    public static IDatabase? Database =>  RedisConn?.GetDatabase();
    
    public const int MaxStreamLength = 1024;
    public const string KumaRequestHandlerStreamName = "kuma:requests";
    
    private const string KumaDiscordStreamName = "kuma:discord:responses";
    private const string KumaTelegramStreamName = "kuma:telegram:responses";
    private const string KumaTerminalStreamName = "kuma:terminal:responses";
    private const string KumaTwitchStreamName = "kuma:twitch:responses";
    
    private static readonly ConnectionMultiplexer? RedisConn;

    static Redis()
    {
        try
        {
            ConfigurationOptions redisConfig = new()
            {
                Password = KumaConfig.RedisPassword,
                AbortOnConnectFail = false
            };
            redisConfig.EndPoints.Add(KumaConfig.RedisHost);
            
            RedisConn = ConnectionMultiplexer.Connect(redisConfig);
        }
        catch (Exception ex)
        {
            Logging.LogExceptionToDatabase(ex, "Unable to connect to Redis");
        }
    }

    /// <summary>
    /// Pings Redis.
    /// </summary>
    /// <returns><see langword="true" /> if successful, <see langword="false" /> otherwise.</returns>
    public static async Task<bool> PingRedisAsync()
    {
        if (!IsConnected) return false;

        IDatabase? db = Database;
        if (db == null) return false;
        
        TimeSpan? result = await db.PingAsync();
        Log.Verbose("Redis PING result: {Result}", result);

        return result != null;
    }

    /// <summary>
    /// Gets the stream name for the provided source system.
    /// </summary>
    /// <param name="sourceSystem">The source system to parse.</param>
    /// <returns>The stream name for the provided source system.</returns>
    public static string GetStreamNameForSourceSystem(SourceSystem sourceSystem)
    {
        return sourceSystem switch
        {
            SourceSystem.Discord => KumaDiscordStreamName,
            SourceSystem.Telegram => KumaTelegramStreamName,
            SourceSystem.Terminal => KumaTerminalStreamName,
            SourceSystem.Twitch => KumaTwitchStreamName,
            _ => throw new ArgumentOutOfRangeException(nameof(sourceSystem), sourceSystem, null)
        };
    }

    /// <summary>
    /// Adds a request to the Redis consumer stream for processing.
    /// </summary>
    /// <param name="kumaRequest">The request to send.</param>
    public static async Task AddRequestToStreamAsync(KumaRequest kumaRequest)
    {
        IDatabase? db = Database;
        if (db == null) return;
        
        string serializedRequest = JsonSerializer.Serialize(kumaRequest);

        Log.Verbose("Sending request to {StreamName}", KumaRequestHandlerStreamName);
        
        await db.StreamAddAsync(
            KumaRequestHandlerStreamName,
            [new NameValueEntry("request", serializedRequest)],
            maxLength: MaxStreamLength,
            useApproximateMaxLength: true);
    }

    /// <summary>
    /// Sends a message to the admin via Telegram.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public static async Task SendNotificationToAdminAsync(string message)
    {
        IDatabase? db = Database;
        if (db == null) return;
        if (KumaConfig.TelegramAdminId == null) return;

        KumaResponse kumaResponse = new(message)
        {
            SourceSystem = SourceSystem.Telegram,
            ChannelId = KumaConfig.TelegramAdminId.ToString()
        };
        
        string serializedRequest = JsonSerializer.Serialize(kumaResponse);

        await db.StreamAddAsync(
            KumaTelegramStreamName,
            [new NameValueEntry("response", serializedRequest)],
            maxLength: MaxStreamLength,
            useApproximateMaxLength: true);
    }

    public static void SendNotificationToAdmin(string message)
    {
        IDatabase? db = Database;
        if (db == null) return;
        if (KumaConfig.TelegramAdminId == null) return;

        KumaResponse kumaResponse = new(message)
        {
            SourceSystem = SourceSystem.Telegram,
            ChannelId = KumaConfig.TelegramAdminId.ToString()
        };
        
        string serializedRequest = JsonSerializer.Serialize(kumaResponse);

        db.StreamAdd(
            KumaTelegramStreamName,
            [new NameValueEntry("response", serializedRequest)],
            maxLength: MaxStreamLength,
            useApproximateMaxLength: true);
    }

    /// <summary>
    /// Sends a deployment notification to the admin via Telegram.
    /// </summary>
    public static async Task SendDeploymentNotificationToAdmin()
    {
        string message = $"{KumaConfig.ApplicationName} `{KumaConfig.ApplicationVersion}` deployed\n\nCommit `{KumaConfig.BuildCommit}`\nHost `{Environment.MachineName}`";
        await SendNotificationToAdminAsync(message);
    }

    internal static RedisDistributedLock? GetRedisDistributedLock(string name)
    {
        if (RedisConn == null || !IsConnected) return null;
        return new RedisDistributedLock(name, RedisConn.GetDatabase());
    }

    internal static string[] GetCachedKeys(string prefix)
    {
        EndPoint? endpoint = RedisConn?.GetEndPoints().First();
        return RedisConn?
            .GetServer(endpoint)
            .Keys(pattern: $"{prefix}:*")
            .Select(x => x.ToString())
            .ToArray() ?? [];
    }

    internal static async Task<bool> SetAsync(string key, object value, TimeSpan? expires = null)
    {
        IDatabase? db = Database;
        if (db == null) return false;

        string? valueToStore;
        if (value is string valueString) valueToStore = valueString;
        else valueToStore = JsonSerializer.Serialize(value);
                
        if (!string.IsNullOrEmpty(valueToStore)) await db.StringSetAsync(key, valueToStore, expires);

        return true;
    }
    
    internal static async Task<string?> GetAsync(string key)
    {
        IDatabase? db = Database;
        if (db == null) return default;

        return await db.StringGetAsync(key);
    }
    
    internal static async Task<T?> GetAsync<T>(string key)
    {
        IDatabase? db = Database;
        if (db == null) return default;

        string? result = await db.StringGetAsync(key);
        return !string.IsNullOrEmpty(result) ? JsonSerializer.Deserialize<T>(result) : default;
    }
    
    internal static async Task<bool> DeleteAsync(string key)
    {
        IDatabase? db = Database;
        if (db == null) return false;

        await db.KeyDeleteAsync(key);
        return true;
    }
}