namespace Doner.Features.MarkdownFeature;

/// <summary>
/// Result of applying an operation to a markdown document
/// </summary>
public class ApplyOperationResult
{
    /// <summary>
    /// Whether the operation was applied successfully
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// The new version of the document after applying the operation
    /// </summary>
    public int NewVersion { get; set; }
    
    /// <summary>
    /// Whether the failure was due to a locking issue
    /// </summary>
    public bool IsLockingIssue { get; set; }
}

/// <summary>
/// Result of processing an operation (transform + apply)
/// </summary>
public class ProcessOperationResult
{
    /// <summary>
    /// Whether the operation was processed successfully
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// The transformed operation that was applied
    /// </summary>
    public Operation? TransformedOperation { get; set; }
    
    /// <summary>
    /// The new version of the document after processing the operation
    /// </summary>
    public int NewVersion { get; set; }
    
    /// <summary>
    /// Whether the failure was due to a locking issue
    /// </summary>
    public bool IsLockingIssue { get; set; }
}
