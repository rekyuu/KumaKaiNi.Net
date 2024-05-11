using System.Runtime.Caching;

namespace KumaKaiNi.Core.Utility;

public static class Cache
{
    private static readonly ObjectCache InMemoryCache;

    static Cache()
    {
        InMemoryCache = MemoryCache.Default;
    }

    /// <summary>
    /// Gets all the stored cache keys.
    /// </summary>
    /// <param name="prefix">The key prefix.</param>
    /// <returns>Array of the matching cache keys.</returns>
    public static string[] GetCachedKeys(string prefix)
    {
        if (Redis.IsConnected) return Redis.GetCachedKeys(prefix);
        
        return InMemoryCache
            .Select(x => x.Key)
            .Where(x => x.StartsWith(prefix))
            .ToArray();
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
            if (Redis.IsConnected) return await Redis.SetAsync(key, value, expires);

            CacheItemPolicy policy = new();
            if (expires != null) policy.AbsoluteExpiration = DateTimeOffset.UtcNow.Add(expires.Value);
            
            InMemoryCache.Set(key, value, policy);

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
            if (Redis.IsConnected) return await Redis.GetAsync(key);
            return (string)InMemoryCache.Get(key);
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
            if (Redis.IsConnected) return await Redis.GetAsync<T>(key);
            return (T)InMemoryCache.Get(key);
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
            if (Redis.IsConnected) return await Redis.DeleteAsync(key);
            
            InMemoryCache.Remove(key);
            return true;
        }
        catch (Exception ex)
        {
            await Logging.LogExceptionToDatabaseAsync(ex, "There was an exception while deleting from cache for {Key}", key);
            return false;
        }
    }
}