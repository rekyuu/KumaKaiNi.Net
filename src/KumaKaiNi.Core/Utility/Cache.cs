using System.Net;
using System.Runtime.Caching;
using System.Text.Json;
using Medallion.Threading.Redis;
using StackExchange.Redis;

namespace KumaKaiNi.Core.Utility;

public static class Cache
{
    private static readonly ConnectionMultiplexer? RedisConn;
    private static bool RedisIsConnected => RedisConn is { IsConnected: true };

    private static readonly ObjectCache MemoryCache;

    static Cache()
    {
        MemoryCache = System.Runtime.Caching.MemoryCache.Default;

        try
        {
            ConfigurationOptions redisConfig = new()
            {
                Password = KumaConfig.RedisPassword,
                AbortOnConnectFail = true
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
        if (!RedisIsConnected) return false;

        IDatabase? db = RedisConn?.GetDatabase();
        if (db == null) return false;
        
        TimeSpan? result = await db.PingAsync();

        return result != null;
    }

    /// <summary>
    /// Gets a <see cref="RedisDistributedLock"/> using the Redis database if connected.
    /// </summary>
    /// <param name="name">The name of the lock.</param>
    /// <returns>The requested <see cref="RedisDistributedLock"/> on success, <see langword="null"/> otherwise.</returns>
    public static RedisDistributedLock? GetRedisDistributedLock(string name)
    {
        if (RedisConn == null || !RedisIsConnected) return null;
        return new RedisDistributedLock(name, RedisConn.GetDatabase());
    }

    /// <summary>
    /// Gets all the stored cache keys.
    /// </summary>
    /// <param name="prefix">The key prefix.</param>
    /// <returns>Array of the matching cache keys.</returns>
    public static string[] GetCachedKeys(string prefix)
    {
        if (!RedisIsConnected)
        {
            return MemoryCache
                .Select(x => x.Key)
                .Where(x => x.StartsWith(prefix))
                .ToArray();
        }

        EndPoint? endpoint = RedisConn?.GetEndPoints().First();
        return RedisConn?
            .GetServer(endpoint)
            .Keys(pattern: $"{prefix}:*")
            .Select(x => x.ToString())
            .ToArray() ?? [];
    }

    /// <summary>
    /// Stores a value to the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The cache object.</param>
    /// <param name="expires">When the cache object should expire.</param>
    /// <returns><see langword="true" /> on success, <see langword="false" /> otherwise.</returns>
    public static async Task<bool> SetAsync(string key, object value, TimeSpan? expires = null)
    {
        try
        {
            if (!RedisIsConnected)
            {
                CacheItemPolicy policy = new();
                if (expires != null) policy.AbsoluteExpiration = DateTimeOffset.UtcNow.Add(expires.Value);
            
                MemoryCache.Set(key, value, policy);

                return true;
            }

            IDatabase? db = RedisConn?.GetDatabase();
            if (db == null) return false;

            string? valueToStore;
            if (value is string valueString) valueToStore = valueString;
            else valueToStore = JsonSerializer.Serialize(value);
                
            if (!string.IsNullOrEmpty(valueToStore)) await db.StringSetAsync(key, valueToStore, expires);

            return true;
        }
        catch (Exception ex)
        {
            await Logging.LogExceptionToDatabaseAsync(ex, "There was an exception while setting to cache for {Key}: {Value}", key, value);
            return false;
        }
    }

    /// <summary>
    /// Retrieves a string value from cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The string object if it exists, <see langword="null" /> otherwise.</returns>
    public static async Task<string?> GetAsync(string key)
    {
        try
        {
            if (!RedisIsConnected) return (string)MemoryCache.Get(key);

            IDatabase? db = RedisConn?.GetDatabase();
            if (db == null) return default;

            return await db.StringGetAsync(key);
        }
        catch (Exception ex)
        {
            await Logging.LogExceptionToDatabaseAsync(ex, "There was an exception while getting from cache for {Key}", key);
            return default;
        }
    }

    /// <summary>
    /// Retrieves an object value from cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The <see cref="T"/> object if it exists, <see langword="null" /> otherwise.</returns>
    public static async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            if (!RedisIsConnected) return (T)MemoryCache.Get(key);

            IDatabase? db = RedisConn?.GetDatabase();
            if (db == null) return default;

            string? result = await db.StringGetAsync(key);
            return !string.IsNullOrEmpty(result) ? JsonSerializer.Deserialize<T>(result) : default;
        }
        catch (Exception ex)
        {
            await Logging.LogExceptionToDatabaseAsync(ex, "There was an exception while getting from cache for {Key}", key);
            return default;
        }
    }

    /// <summary>
    /// Deletes an entry from the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns><see langword="true" /> on success, <see langword="false" /> otherwise.</returns>
    public static async Task<bool> DeleteAsync(string key)
    {
        try
        {
            if (!RedisIsConnected)
            {
                MemoryCache.Remove(key);
                return true;
            }

            IDatabase? db = RedisConn?.GetDatabase();
            if (db == null) return false;

            await db.KeyDeleteAsync(key);
            return true;
        }
        catch (Exception ex)
        {
            await Logging.LogExceptionToDatabaseAsync(ex, "There was an exception while deleting from cache for {Key}", key);
            return false;
        }
    }
}