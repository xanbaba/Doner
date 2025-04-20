namespace Doner.Features.MarkdownFeature;

/// <summary>
/// Manages editing sessions for markdown documents
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Opens a new editing session for a markdown document or refreshes an existing one
    /// </summary>
    /// <param name="markdownId">The ID of the markdown document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task OpenSessionAsync(string markdownId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Closes an editing session for a markdown document
    /// </summary>
    /// <param name="markdownId">The ID of the markdown document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CloseSessionAsync(string markdownId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a session is currently active for a markdown document
    /// </summary>
    /// <param name="markdownId">The ID of the markdown document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the session is active, false otherwise</returns>
    Task<bool> IsSessionActiveAsync(string markdownId, CancellationToken cancellationToken = default); 
    
    /// <summary>
    /// Refreshes an existing session to prevent it from expiring
    /// </summary>
    /// <param name="markdownId">The ID of the markdown document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RefreshSessionAsync(string markdownId, CancellationToken cancellationToken = default);
}
