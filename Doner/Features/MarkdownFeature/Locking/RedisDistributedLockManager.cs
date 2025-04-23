using System.Collections.Concurrent;
using System.Diagnostics;
using StackExchange.Redis;

namespace Doner.Features.MarkdownFeature.Locking;

/// <summary>
/// Redis implementation of distributed lock manager
/// </summary>
public class RedisDistributedLockManager : IDistributedLockManager
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisDistributedLockManager> _logger;
    private readonly ConcurrentDictionary<string, bool> _activeLocks = new();
    
    // Default lock expiration time (30 seconds)
    private static readonly TimeSpan DefaultLockExpiry = TimeSpan.FromSeconds(30);
    
    public RedisDistributedLockManager(
        IConnectionMultiplexer redis,
        ILogger<RedisDistributedLockManager> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Attempt to acquire a lock for the specified resource
    /// </summary>
    /// <param name="resourceKey">Unique key identifying the resource to lock</param>
    /// <param name="timeout">How long to wait for lock acquisition before timing out</param>
    /// <param name="cancellationToken">Token to cancel the lock acquisition attempt</param>
    /// <returns>A distributed lock if successful</returns>
    /// <exception cref="TimeoutException">Thrown if the lock cannot be acquired within the timeout period</exception>
    public async Task<IDistributedLock> AcquireLockAsync(
        string resourceKey, 
        TimeSpan timeout, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(resourceKey))
        {
            throw new ArgumentException("Resource key cannot be null or empty", nameof(resourceKey));
        }
        
        // Prepend namespace to avoid conflicts with other Redis keys
        string redisKey = $"lock:{resourceKey}";
        
        // Create a unique ID for this lock instance
        string lockId = Guid.NewGuid().ToString();
        
        // Create the lock object
        var redisLock = new RedisDistributedLock(_redis, redisKey, lockId);
        
        // Default retry interval
        var retryInterval = TimeSpan.FromMilliseconds(100);
        
        // Start timing the lock acquisition
        var stopwatch = Stopwatch.StartNew();

        // Try to acquire the lock until successful or timeout
        while (stopwatch.Elapsed < timeout)
        {
            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();
            
            try
            {
                // Try to acquire the lock
                var acquired = await redisLock.AcquireAsync(DefaultLockExpiry, cancellationToken);
                
                if (acquired)
                {
                    _logger.LogDebug("Lock acquired for resource {ResourceKey} with lock ID {LockId}", 
                        resourceKey, lockId);
                    
                    // Record the active lock
                    _activeLocks[resourceKey] = true;
                    
                    // Return the lock
                    return redisLock;
                }
                
                // Wait before retrying
                await Task.Delay(retryInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error attempting to acquire lock for resource {ResourceKey}: {Message}", 
                    resourceKey, ex.Message);
                
                // Wait before retrying after an error
                await Task.Delay(retryInterval, cancellationToken);
            }
        }
        
        // If we got here, we couldn't acquire the lock within the timeout period
        throw new TimeoutException($"Unable to acquire lock on resource '{resourceKey}' within timeout of {timeout.TotalMilliseconds}ms");
    }
    
    /// <summary>
    /// Check if a resource is currently locked
    /// </summary>
    /// <param name="resourceKey">The resource key to check</param>
    /// <returns>True if the resource is locked, false otherwise</returns>
    public bool IsLocked(string resourceKey)
    {
        if (string.IsNullOrEmpty(resourceKey))
        {
            throw new ArgumentException("Resource key cannot be null or empty", nameof(resourceKey));
        }
        
        // First check our local cache of active locks
        if (_activeLocks.TryGetValue(resourceKey, out bool isLocked) && isLocked)
        {
            return true;
        }
        
        try
        {
            // Check Redis directly
            string redisKey = $"lock:{resourceKey}";
            var database = _redis.GetDatabase();
            return database.KeyExists(redisKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking lock status for resource {ResourceKey}: {Message}", 
                resourceKey, ex.Message);
            
            // If we can't check, assume it's not locked
            return false;
        }
    }
}
