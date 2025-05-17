namespace Doner.Features.MarkdownFeature.Repositories;

public interface IMarkdownRepository
{
    // Original methods
    Task<IEnumerable<Markdown>> GetMarkdownsByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<string> CreateMarkdownAsync(string title, Guid ownerId, Guid workspaceId, CancellationToken cancellationToken = default);
    Task UpdateMarkdownAsync(string markdownId, string title, CancellationToken cancellationToken = default);
    Task DeleteMarkdownAsync(string markdownId, CancellationToken cancellationToken = default);
    
    // New methods for OT operations
    /// <summary>
    /// Gets markdown metadata without the content
    /// </summary>
    Task<MarkdownMetadata?> GetMarkdownMetadataAsync(string markdownId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if the markdown document is of the expected version
    /// </summary>
    Task<bool> CheckVersionAsync(string markdownId, int expectedVersion, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Increments the version of the markdown document
    /// </summary>
    Task IncrementVersionAsync(string markdownId, CancellationToken cancellationToken = default);
    
    // Content-specific operations
    /// <summary>
    /// Inserts text at the specified position in the markdown content
    /// This MUST directly modify the content in the database without loading the entire content
    /// </summary>
    /// <param name="markdownId">The ID of the markdown document</param>
    /// <param name="position">The position at which to insert the text</param>
    /// <param name="text">The text to insert</param>
    /// <param name="cancellationToken"></param>
    /// <returns>True if the operation succeeded, false otherwise</returns>
    Task<bool> InsertContentAsync(string markdownId, int position, string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes characters at the specified position in the markdown content
    /// This MUST directly modify the content in the database without loading the entire content
    /// </summary>
    /// <param name="markdownId">The ID of the markdown document</param>
    /// <param name="position">The position from which to delete characters</param>
    /// <param name="count">The number of characters to delete</param>
    /// <param name="cancellationToken"></param>
    /// <returns>True if the operation succeeded, false otherwise</returns>
    Task<bool> DeleteContentAsync(string markdownId, int position, int count, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current state of a document including content and version
    /// </summary>
    /// <param name="markdownId">The ID of the markdown document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document state with content and version, or null if document doesn't exist</returns>
    Task<DocumentState?> GetDocumentStateAsync(string markdownId, CancellationToken cancellationToken = default);
}

public class DocumentState
{
    public string Content { get; set; } = string.Empty;
    public int Version { get; set; }
}