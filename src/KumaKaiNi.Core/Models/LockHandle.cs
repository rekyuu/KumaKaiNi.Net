using KumaKaiNi.Core.Utility;
using Medallion.Threading.Redis;

namespace KumaKaiNi.Core.Models;

/// <summary>
/// Lock object that utilizes both local semaphores and remote Redis distributed locking.
/// </summary>
/// <param name="name">The name of the lock.</param>
public class LockHandle(string name) : IDisposable, IAsyncDisposable
{
    private const int DefaultTimeout = 30_000;
    
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly RedisDistributedLock? _redisLock = Cache.GetRedisDistributedLock(name);
    private RedisDistributedLockHandle? _redisLockHandle;

    /// <summary>
    /// Blocks the thread until a lock can be obtained.
    /// </summary>
    /// <param name="millisecondsTimeout">Time to wait before timeout.</param>
    /// <returns><see langword="true" /> if the lock was obtained, <see langword="false" /> otherwise.</returns>
    public bool TryAcquire(int millisecondsTimeout = DefaultTimeout)
    {
        TimeSpan timeout = millisecondsTimeout < 0 ? TimeSpan.MaxValue : TimeSpan.FromMilliseconds(millisecondsTimeout);
        return TryAcquire(timeout);
    }

    /// <summary>
    /// Blocks the thread until a lock can be obtained.
    /// </summary>
    /// <param name="timeout">Time to wait before timeout.</param>
    /// <returns><see langword="true" /> if the lock was obtained, <see langword="false" /> otherwise.</returns>
    public bool TryAcquire(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromMilliseconds(DefaultTimeout);
        
        bool semaphoreLocked = _semaphore.Wait(timeout.Value);

        if (_redisLock == null) return semaphoreLocked;
            
        _redisLockHandle = _redisLock.TryAcquire(timeout.Value);
        return semaphoreLocked && _redisLockHandle != null;
    }

    /// <summary>
    /// Asynchronously waits until a lock can be obtained.
    /// </summary>
    /// <param name="millisecondsTimeout">Time to wait before timeout.</param>
    /// <returns><see langword="true" /> if the lock was obtained, <see langword="false" /> otherwise.</returns>
    public async Task<bool> TryAcquireAsync(int millisecondsTimeout = DefaultTimeout)
    {
        TimeSpan timeout = millisecondsTimeout < 0 ? TimeSpan.MaxValue : TimeSpan.FromMilliseconds(millisecondsTimeout);
        return await TryAcquireAsync(timeout);
    }

    /// <summary>
    /// Asynchronously waits until a lock can be obtained.
    /// </summary>
    /// <param name="timeout">Time to wait before timeout.</param>
    /// <returns><see langword="true" /> if the lock was obtained, <see langword="false" /> otherwise.</returns>
    public async Task<bool> TryAcquireAsync(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromMilliseconds(DefaultTimeout);
        
        bool semaphoreLocked = await _semaphore.WaitAsync(timeout.Value);

        if (_redisLock == null) return semaphoreLocked;
        
        _redisLockHandle = await _redisLock.TryAcquireAsync(timeout.Value);
        return semaphoreLocked && _redisLockHandle != null;
    }
    
    public void Dispose()
    {
        _semaphore.Release();
        _redisLockHandle?.Dispose();
        
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        _semaphore.Release();
        if (_redisLockHandle != null) await _redisLockHandle.DisposeAsync();
        
        GC.SuppressFinalize(this);
    }
}