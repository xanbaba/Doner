using Doner.Features.MarkdownFeature.Repositories;

namespace Doner.Features.MarkdownFeature.OT;

/// <summary>
/// Service for handling Operational Transformation operations
/// </summary>
public class OTService : IOTService
{
    private readonly IOperationRepository _operationRepository;
    private readonly IMarkdownRepository _markdownRepository;
    private readonly IOTProcessor _otProcessor;

    public OTService(
        IOperationRepository operationRepository, 
        IMarkdownRepository markdownRepository,
        IOTProcessor otProcessor)
    {
        _operationRepository = operationRepository;
        _markdownRepository = markdownRepository;
        _otProcessor = otProcessor;
    }

    private async Task ApplyOperationAsync(string markdownId,
        Operation operation,
        CancellationToken cancellationToken = default)
    {
        // Check if markdown exists and get its metadata (without content)
        var metadata = await _markdownRepository.GetMarkdownMetadataAsync(markdownId, cancellationToken);
        
        if (metadata == null)
        {
            return;
        }
    
        // Check version - this check should be protected by a lock at the caller level
        if (operation.BaseVersion != metadata.Version)
        {
            return;
        }
    
        // Apply operation directly to the storage using the processor
        if (!await _otProcessor.ApplyComponentsAsync(markdownId, operation.Components, cancellationToken))
        {
            return;
        }
        
        // Increment version
        await _markdownRepository.IncrementVersionAsync(markdownId, cancellationToken);
        
        // Save the operation
        await _operationRepository.AddOperationAsync(operation, cancellationToken);
    }

    public async Task<Operation?> ProcessOperationAsync(
        Operation operation, 
        CancellationToken cancellationToken = default)
    {
        // First, check if the document exists before proceeding with any transformations
        var markdownExists = await _markdownRepository.GetMarkdownMetadataAsync(
            operation.MarkdownId, cancellationToken) != null;
            
        if (!markdownExists)
        {
            return null;
        }
        
        // Get the current version of the document
        int currentVersion = await _operationRepository.GetLatestVersionAsync(
            operation.MarkdownId, cancellationToken);
        
        // If the operation is already based on the latest version, apply it directly
        if (operation.BaseVersion == currentVersion)
        {
            await ApplyOperationAsync(
                operation.MarkdownId, operation, cancellationToken);

            return operation;
        }
        
        // Otherwise, the operation needs to be transformed against all operations
        // that have been applied since the operation's base version
        var operationsToTransformAgainst = await _operationRepository.GetOperationsAsync(
            operation.MarkdownId, operation.BaseVersion, cancellationToken);
        
        var transformedOperation = operation;
        
        // Transform the operation against all server operations
        // This doesn't need locking since it doesn't modify the document
        foreach (var serverOperation in operationsToTransformAgainst.OrderBy(o => o.BaseVersion))
        {
            transformedOperation = _otProcessor.Transform(transformedOperation, serverOperation);
        }
        
        // Apply the transformed operation (this will acquire a lock)
        await ApplyOperationAsync(
            operation.MarkdownId, transformedOperation, cancellationToken);

        return transformedOperation;
    }
}