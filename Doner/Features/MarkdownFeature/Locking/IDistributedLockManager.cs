namespace Doner.Features.MarkdownFeature.Locking;

/// <summary>
/// Manager for distributed locks to ensure exclusive access to resources
/// </summary>
public interface IDistributedLockManager
{
    /// <summary>
    /// Attempt to acquire a lock for the specified resource
    /// </summary>
    /// <param name="resourceKey">Unique key identifying the resource to lock</param>
    /// <param name="timeout">How long to wait for lock acquisition before timing out</param>
    /// <param name="cancellationToken">Token to cancel the lock acquisition attempt</param>
    /// <returns>A distributed lock if successful, null if unable to acquire within timeout</returns>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled</exception>
    /// <exception cref="TimeoutException">Thrown if the lock cannot be acquired within the timeout period</exception>
    Task<IDistributedLock> AcquireLockAsync(
        string resourceKey, 
        TimeSpan timeout, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a resource is currently locked
    /// </summary>
    /// <param name="resourceKey">The resource key to check</param>
    /// <returns>True if the resource is locked, false otherwise</returns>
    bool IsLocked(string resourceKey);
}
