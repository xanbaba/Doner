using Doner.Features.MarkdownFeature.Locking;

namespace Doner.Features.MarkdownFeature;

/// <summary>
/// Service for handling Operational Transformation operations
/// </summary>
public class OTService : IOTService
{
    private readonly IOperationRepository _operationRepository;
    private readonly IMarkdownRepository _markdownRepository;
    private readonly IOTProcessor _otProcessor;
    private readonly IDistributedLockManager _lockManager;
    
    // Default timeout for acquiring a document lock
    private static readonly TimeSpan DefaultLockTimeout = TimeSpan.FromSeconds(30);

    public OTService(
        IOperationRepository operationRepository, 
        IMarkdownRepository markdownRepository,
        IOTProcessor otProcessor,
        IDistributedLockManager lockManager)
    {
        _operationRepository = operationRepository;
        _markdownRepository = markdownRepository;
        _otProcessor = otProcessor;
        _lockManager = lockManager;
    }

    public Operation Transform(Operation clientOperation, Operation serverOperation)
    {
        return _otProcessor.Transform(clientOperation, serverOperation);
    }

    public async Task<ApplyOperationResult> ApplyOperationAsync(
        string markdownId, 
        Operation operation, 
        CancellationToken cancellationToken = default)
    {
        // Create a resource key for this document
        string lockKey = $"markdown:{markdownId}";
        
        try
        {
            // Acquire a lock for this document
            await using var docLock = await _lockManager.AcquireLockAsync(
                lockKey, DefaultLockTimeout, cancellationToken);
                
            // Now we have exclusive access to modify this document
            
            // Check if markdown exists and get its metadata (without content)
            var metadata = await _markdownRepository.GetMarkdownMetadataAsync(markdownId, cancellationToken);
            
            if (metadata == null)
            {
                return new ApplyOperationResult 
                { 
                    Success = false, 
                    ErrorMessage = "Markdown document not found" 
                };
            }
        
            // Check version - this check is now reliable because we have a lock
            if (operation.BaseVersion != metadata.Version)
            {
                return new ApplyOperationResult 
                { 
                    Success = false, 
                    ErrorMessage = "Operation base version does not match document version" 
                };
            }
        
            // Apply operation directly to the storage using the processor
            if (!await _otProcessor.ApplyComponentsAsync(markdownId, operation.Components, cancellationToken))
            {
                return new ApplyOperationResult 
                { 
                    Success = false, 
                    ErrorMessage = "Failed to apply operation to document content" 
                };
            }
            
            // Increment version
            await _markdownRepository.IncrementVersionAsync(markdownId, cancellationToken);
            
            // Save the operation
            await _operationRepository.AddOperationAsync(operation, cancellationToken);
            
            // We can confidently set the new version to be the base version + 1
            // since we hold an exclusive lock and just incremented it
            int newVersion = metadata.Version + 1;
            
            return new ApplyOperationResult { Success = true, NewVersion = newVersion };
            
            // The lock will be automatically released when exiting the using block
        }
        catch (TimeoutException)
        {
            // Could not acquire the lock within the timeout period
            return new ApplyOperationResult 
            { 
                Success = false, 
                ErrorMessage = "Document is currently being modified by another operation. Please try again." 
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // An unexpected error occurred
            return new ApplyOperationResult 
            { 
                Success = false, 
                ErrorMessage = $"An error occurred while applying the operation: {ex.Message}" 
            };
        }
    }

    public async Task<ProcessOperationResult> ProcessOperationAsync(
        Operation operation, 
        CancellationToken cancellationToken = default)
    {
        // First, check if the document exists before proceeding with any transformations
        var markdownExists = await _markdownRepository.GetMarkdownMetadataAsync(
            operation.MarkdownId, cancellationToken) != null;
            
        if (!markdownExists)
        {
            return new ProcessOperationResult 
            { 
                Success = false,
                ErrorMessage = "Markdown document not found",
                TransformedOperation = null,
                NewVersion = 0
            };
        }
        
        // Get the current version of the document
        int currentVersion = await _operationRepository.GetLatestVersionAsync(
            operation.MarkdownId, cancellationToken);
        
        // If the operation is already based on the latest version, apply it directly
        if (operation.BaseVersion == currentVersion)
        {
            var applyResult = await ApplyOperationAsync(
                operation.MarkdownId, operation, cancellationToken);
            
            return new ProcessOperationResult 
            { 
                Success = applyResult.Success,
                ErrorMessage = applyResult.ErrorMessage,
                TransformedOperation = operation,
                NewVersion = applyResult.NewVersion
            };
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
        var result = await ApplyOperationAsync(
            operation.MarkdownId, transformedOperation, cancellationToken);
        
        return new ProcessOperationResult 
        { 
            Success = result.Success,
            ErrorMessage = result.ErrorMessage,
            TransformedOperation = transformedOperation,
            NewVersion = result.NewVersion
        };
    }
}