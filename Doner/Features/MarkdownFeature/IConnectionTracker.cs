namespace Doner.Features.MarkdownFeature;

/// <summary>
/// Tracks connections between users and documents for real-time collaboration
/// </summary>
public interface IConnectionTracker
{
    /// <summary>
    /// Tracks a new connection to a document
    /// </summary>
    /// <param name="connectionId">The SignalR connection ID</param>
    /// <param name="documentId">The document ID the user is viewing/editing</param>
    /// <param name="userId">ID of the user connecting to the document</param>
    Task TrackConnectionAsync(string connectionId, string documentId, Guid userId);
    
    /// <summary>
    /// Gets the document ID associated with a connection
    /// </summary>
    /// <param name="connectionId">The SignalR connection ID</param>
    /// <returns>The document ID or null if not found</returns>
    Task<string?> GetDocumentForConnectionAsync(string connectionId);
    
    /// <summary>
    /// Gets all connection IDs for users connected to a document
    /// </summary>
    /// <param name="documentId">The document ID</param>
    /// <returns>Collection of connection IDs</returns>
    Task<IEnumerable<string>> GetConnectionsForDocumentAsync(string documentId);
    
    /// <summary>
    /// Removes a connection from tracking when it disconnects
    /// </summary>
    /// <param name="connectionId">The SignalR connection ID to remove</param>
    Task RemoveConnectionAsync(string connectionId);
    
    /// <summary>
    /// Gets information about the user associated with a connection
    /// </summary>
    /// <param name="connectionId">The SignalR connection ID</param>
    /// <returns>User information or null if not found</returns>
    Task<UserInfo?> GetUserInfoAsync(string connectionId);
    
    /// <summary>
    /// Gets all documents that currently have active connections
    /// </summary>
    /// <returns>Collection of document IDs</returns>
    Task<IEnumerable<string>> GetActiveDocumentsAsync();
}
