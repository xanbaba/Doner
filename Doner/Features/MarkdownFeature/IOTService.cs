namespace Doner.Features.MarkdownFeature;

/// <summary>
/// Service for handling Operational Transformation operations
/// </summary>
public interface IOTService
{
    /// <summary>
    /// Transform an operation against another operation
    /// </summary>
    /// <param name="clientOperation">Client operation to be transformed</param>
    /// <param name="serverOperation">Server operation to transform against</param>
    /// <returns>A new operation with transformed components</returns>
    Operation Transform(Operation clientOperation, Operation serverOperation);
    
    /// <summary>
    /// Apply an operation to a markdown document
    /// </summary>
    /// <param name="markdownId">ID of the markdown document</param>
    /// <param name="operation">Operation to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation application</returns>
    Task<ApplyOperationResult> ApplyOperationAsync(string markdownId, Operation operation, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Process an incoming operation (transform if needed, then apply)
    /// </summary>
    /// <param name="operation">Operation to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation processing</returns>
    Task<ProcessOperationResult> ProcessOperationAsync(Operation operation, CancellationToken cancellationToken = default);
}