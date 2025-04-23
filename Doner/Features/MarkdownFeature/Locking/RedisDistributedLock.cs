using StackExchange.Redis;

namespace Doner.Features.MarkdownFeature.Locking;

/// <summary>
/// Redis implementation of a distributed lock
/// </summary>
public class RedisDistributedLock : IDistributedLock
{
    private readonly IDatabase _database;
    private readonly string _resourceKey;
    private readonly string _lockId;
    private bool _isValid;
    
    // Lua script to release the lock in an atomic way
    // This ensures that we only release locks that we own (matching lockId)
    private const string ReleaseLockScript = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end";
    
    public RedisDistributedLock(
        IConnectionMultiplexer redis,
        string resourceKey,
        string lockId)
    {
        _database = redis.GetDatabase();
        _resourceKey = resourceKey ?? throw new ArgumentNullException(nameof(resourceKey));
        _lockId = lockId ?? throw new ArgumentNullException(nameof(lockId));
        _isValid = true;
    }
    
    /// <summary>
    /// Gets the resource key this lock is protecting
    /// </summary>
    public string ResourceKey => _resourceKey;
    
    /// <summary>
    /// Gets whether this lock is still valid
    /// </summary>
    public bool IsValid => _isValid && CheckIfLockExists();
    
    /// <summary>
    /// Attempts to acquire the Redis lock
    /// </summary>
    /// <param name="expiry">The expiration time for the lock</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the lock was acquired, false otherwise</returns>
    internal async Task<bool> AcquireAsync(TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        // Use SET with NX (not exists) option for atomic lock acquisition
        bool acquired = await _database.StringSetAsync(
            _resourceKey,
            _lockId,
            expiry,
            When.NotExists,
            CommandFlags.None);
        
        _isValid = acquired;
        return acquired;
    }
    
    /// <summary>
    /// Release the lock before disposal
    /// </summary>
    /// <returns>A task representing the release operation</returns>
    public async Task ReleaseAsync()
    {
        if (!_isValid)
        {
            return;
        }
        
        try
        {
            // Execute Lua script to release lock only if we own it
            var result = await _database.ScriptEvaluateAsync(
                ReleaseLockScript,
                [_resourceKey],
                [_lockId]);
            
            bool released = (long)result == 1;
            
            if (released)
            {
                _isValid = false;
            }
        }
        catch (Exception)
        {
            // Best effort to release the lock
            // If it fails, the lock will eventually expire
        }
    }
    
    /// <summary>
    /// Check if the lock still exists in Redis
    /// </summary>
    /// <returns>True if the lock exists and has our lock ID, false otherwise</returns>
    private bool CheckIfLockExists()
    {
        try
        {
            var value = _database.StringGet(_resourceKey);
            return value.HasValue && value.ToString() == _lockId;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Disposes the lock, releasing it if it's currently valid
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await ReleaseAsync();
    }
}
