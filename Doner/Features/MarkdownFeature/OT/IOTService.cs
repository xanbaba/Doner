namespace Doner.Features.MarkdownFeature.OT;

/// <summary>
/// Service for handling Operational Transformation operations
/// </summary>
public interface IOTService
{
    /// <summary>
    /// Process an incoming operation (transform if needed, then apply)
    /// </summary>
    /// <param name="operation">Operation to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processed operation or null if operation couldn't be processed</returns>
    Task<Operation?> ProcessOperationAsync(Operation operation, CancellationToken cancellationToken = default);
}