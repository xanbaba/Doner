namespace Doner.Features.MarkdownFeature.Locking;

/// <summary>
/// Represents a distributed lock that can be released
/// </summary>
public interface IDistributedLock : IAsyncDisposable
{
    /// <summary>
    /// Gets the resource key this lock is protecting
    /// </summary>
    string ResourceKey { get; }
    
    /// <summary>
    /// Gets whether this lock is still valid
    /// </summary>
    bool IsValid { get; }
    
    /// <summary>
    /// Release the lock before disposal
    /// </summary>
    /// <returns>A task representing the release operation</returns>
    Task ReleaseAsync();
}
