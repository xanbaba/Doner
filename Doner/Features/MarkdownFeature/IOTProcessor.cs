namespace Doner.Features.MarkdownFeature;

/// <summary>
/// Interface for Operational Transformation processor that handles
/// transformation and application of operations
/// </summary>
public interface IOTProcessor
{
    /// <summary>
    /// Transforms a client operation against a server operation to preserve the intention of both
    /// </summary>
    /// <param name="clientOperation">Client operation to be transformed</param>
    /// <param name="serverOperation">Server operation to transform against</param>
    /// <returns>A new operation with transformed components</returns>
    Operation Transform(Operation clientOperation, Operation serverOperation);
    
    /// <summary>
    /// Transforms components of a client operation against server operation components
    /// </summary>
    /// <param name="clientComponents">Client operation components to be transformed</param>
    /// <param name="serverComponents">Server operation components to transform against</param>
    /// <returns>Transformed operation components</returns>
    IEnumerable<OperationComponent> TransformComponents(
        IEnumerable<OperationComponent> clientComponents, 
        IEnumerable<OperationComponent> serverComponents);
    
    /// <summary>
    /// Applies operation components to a markdown document
    /// </summary>
    /// <param name="markdownId">ID of the markdown document</param>
    /// <param name="components">Operation components to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> ApplyComponentsAsync(
        string markdownId, 
        IEnumerable<OperationComponent> components,
        CancellationToken cancellationToken = default);
}
