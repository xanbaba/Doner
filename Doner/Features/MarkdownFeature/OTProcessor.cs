namespace Doner.Features.MarkdownFeature;

/// <summary>
/// Implementation of the Operational Transformation processor
/// </summary>
public class OTProcessor : IOTProcessor
{
    private readonly IMarkdownRepository _markdownRepository;

    public OTProcessor(IMarkdownRepository markdownRepository)
    {
        _markdownRepository = markdownRepository;
    }

    public Operation Transform(Operation clientOperation, Operation serverOperation)
    {
        // Create a new operation that will hold the transformed components
        var transformedOperation = new Operation
        {
            Id = Guid.NewGuid(),
            MarkdownId = clientOperation.MarkdownId,
            UserId = clientOperation.UserId,
            BaseVersion = serverOperation.BaseVersion + 1,
            Timestamp = DateTime.UtcNow,
            Components = TransformComponents(clientOperation.Components, serverOperation.Components)
        };

        return transformedOperation;
    }

    public IEnumerable<OperationComponent> TransformComponents(
        IEnumerable<OperationComponent> clientComponents, 
        IEnumerable<OperationComponent> serverComponents)
    {
        var clientComponentsList = clientComponents.ToList();
        var serverComponentsList = serverComponents.ToList();
        
        // Validate components
        ValidateComponents(clientComponentsList);
        ValidateComponents(serverComponentsList);
        
        var result = new List<OperationComponent>();
        
        // Index positions in both component lists
        int clientIndex = 0;
        int serverIndex = 0;
        
        // Iterate until we've processed all components from both operations
        while (clientIndex < clientComponentsList.Count || serverIndex < serverComponentsList.Count)
        {
            // If client operation is exhausted but server isn't, we're done
            if (clientIndex >= clientComponentsList.Count)
            {
                break;
            }
            
            // If server operation is exhausted but client isn't
            if (serverIndex >= serverComponentsList.Count)
            {
                // Add remaining client components to result
                for (int i = clientIndex; i < clientComponentsList.Count; i++)
                {
                    result.Add(clientComponentsList[i]);
                }
                break;
            }
            
            // Get current components
            var clientComponent = clientComponentsList[clientIndex];
            var serverComponent = serverComponentsList[serverIndex];
            
            // Transform current components based on their types
            var transformResult = TransformComponentPair(clientComponent, serverComponent);
            
            // Add transformed components to result
            result.AddRange(transformResult.TransformedClient);
            
            // Update client component if partially consumed
            if (transformResult.ClientRemaining != null)
            {
                clientComponentsList[clientIndex] = transformResult.ClientRemaining;
            }
            else if (transformResult.ClientFullyConsumed)
            {
                clientIndex++;
            }
            
            // Update server component if partially consumed
            if (transformResult.ServerRemaining != null)
            {
                serverComponentsList[serverIndex] = transformResult.ServerRemaining;
            }
            else if (transformResult.ServerFullyConsumed)
            {
                serverIndex++;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Validates operation components to ensure they have valid parameters
    /// </summary>
    private void ValidateComponents(List<OperationComponent> components)
    {
        foreach (var component in components)
        {
            switch (component)
            {
                case RetainComponent retain:
                    if (retain.Count < 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(components), 
                            "Retain component count cannot be negative");
                    }
                    break;
                    
                case DeleteComponent delete:
                    if (delete.Count < 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(components), 
                            "Delete component count cannot be negative");
                    }
                    break;
                    
                case InsertComponent insert:
                    if (insert.Text == null)
                    {
                        throw new ArgumentNullException(nameof(components), 
                            "Insert component text cannot be null");
                    }
                    break;
            }
        }
    }
    
    /// <summary>
    /// Represents the result of transforming a pair of operation components
    /// </summary>
    private class TransformResult
    {
        /// <summary>
        /// The transformed client components
        /// </summary>
        public List<OperationComponent> TransformedClient { get; set; } = new List<OperationComponent>();
        
        /// <summary>
        /// Whether the client component was fully consumed
        /// </summary>
        public bool ClientFullyConsumed { get; set; }
        
        /// <summary>
        /// Whether the server component was fully consumed
        /// </summary>
        public bool ServerFullyConsumed { get; set; }
        
        /// <summary>
        /// The remaining part of the client component, if it was partially consumed
        /// </summary>
        public OperationComponent? ClientRemaining { get; set; }
        
        /// <summary>
        /// The remaining part of the server component, if it was partially consumed
        /// </summary>
        public OperationComponent? ServerRemaining { get; set; }
    }
    
    /// <summary>
    /// Transforms a single client component against a single server component
    /// </summary>
    private TransformResult TransformComponentPair(OperationComponent clientComponent, OperationComponent serverComponent)
    {
        var result = new TransformResult();
        
        // Case 1: Client Retain vs Server Retain
        if (clientComponent is RetainComponent clientRetain && 
            serverComponent is RetainComponent serverRetain)
        {
            int minCount = Math.Min(clientRetain.Count, serverRetain.Count);
            
            result.TransformedClient.Add(new RetainComponent { Count = minCount });
            
            // Update consumption flags
            result.ClientFullyConsumed = (clientRetain.Count <= serverRetain.Count);
            result.ServerFullyConsumed = (serverRetain.Count <= clientRetain.Count);
            
            // If not fully consumed, handle the remaining parts
            if (!result.ClientFullyConsumed)
            {
                result.ClientRemaining = new RetainComponent { Count = clientRetain.Count - serverRetain.Count };
            }
            
            if (!result.ServerFullyConsumed)
            {
                result.ServerRemaining = new RetainComponent { Count = serverRetain.Count - clientRetain.Count };
            }
        }
        
        // Case 2: Client Retain vs Server Insert
        else if (clientComponent is RetainComponent && 
                serverComponent is InsertComponent serverInsert)
        {
            // Client must retain the inserted text
            result.TransformedClient.Add(new RetainComponent { Count = serverInsert.Text.Length });
            
            // Server insert is always fully consumed
            result.ServerFullyConsumed = true;
            
            // Client retain is not consumed at all by server insert
            result.ClientFullyConsumed = false;
        }
        
        // Case 3: Client Retain vs Server Delete
        else if (clientComponent is RetainComponent clientRetain3 && 
                serverComponent is DeleteComponent serverDelete)
        {
            // Client cannot retain deleted characters
            // No component added to transformedClient
            
            // Update consumption flags
            result.ClientFullyConsumed = (clientRetain3.Count <= serverDelete.Count);
            result.ServerFullyConsumed = (serverDelete.Count <= clientRetain3.Count);
            
            // If not fully consumed, handle the remaining parts
            if (!result.ClientFullyConsumed)
            {
                result.ClientRemaining = new RetainComponent { Count = clientRetain3.Count - serverDelete.Count };
            }
            
            if (!result.ServerFullyConsumed)
            {
                result.ServerRemaining = new DeleteComponent { Count = serverDelete.Count - clientRetain3.Count };
            }
        }
        
        // Case 4: Client Insert
        else if (clientComponent is InsertComponent clientInsert)
        {
            // If server is inserting at the same position
            if (serverComponent is InsertComponent serverInsert2)
            {
                // Server has priority, so client insert happens after server insert
                result.TransformedClient.Add(new RetainComponent { Count = serverInsert2.Text.Length });
            }
            
            // Add the client insert
            result.TransformedClient.Add(new InsertComponent { Text = clientInsert.Text });
            
            // Client insert is always fully consumed
            result.ClientFullyConsumed = true;
            
            // Server component is never consumed by client insert
            result.ServerFullyConsumed = false;
        }
        
        // Case 5: Client Delete vs Server Retain
        else if (clientComponent is DeleteComponent clientDelete && 
                serverComponent is RetainComponent serverRetain2)
        {
            int minCount = Math.Min(clientDelete.Count, serverRetain2.Count);
            
            // Client can delete what server retains
            result.TransformedClient.Add(new DeleteComponent { Count = minCount });
            
            // Update consumption flags
            result.ClientFullyConsumed = (clientDelete.Count <= serverRetain2.Count);
            result.ServerFullyConsumed = (serverRetain2.Count <= clientDelete.Count);
            
            // If not fully consumed, handle the remaining parts
            if (!result.ClientFullyConsumed)
            {
                result.ClientRemaining = new DeleteComponent { Count = clientDelete.Count - serverRetain2.Count };
            }
            
            if (!result.ServerFullyConsumed)
            {
                result.ServerRemaining = new RetainComponent { Count = serverRetain2.Count - clientDelete.Count };
            }
        }
        
        // Case 6: Client Delete vs Server Insert
        else if (clientComponent is DeleteComponent && 
                serverComponent is InsertComponent serverInsert3)
        {
            // Client needs to skip over the inserted text before deleting
            result.TransformedClient.Add(new RetainComponent { Count = serverInsert3.Text.Length });
            
            // The delete itself remains unchanged
            // Client delete is not consumed at all
            result.ClientFullyConsumed = false;
            
            // Server insert is fully consumed
            result.ServerFullyConsumed = true;
        }
        
        // Case 7: Client Delete vs Server Delete
        else if (clientComponent is DeleteComponent clientDelete3 && 
                serverComponent is DeleteComponent serverDelete2)
        {
            // No need to delete what server has already deleted
            // No component added to transformedClient
            
            // Update consumption flags
            result.ClientFullyConsumed = (clientDelete3.Count <= serverDelete2.Count);
            result.ServerFullyConsumed = (serverDelete2.Count <= clientDelete3.Count);
            
            // If not fully consumed, handle the remaining parts
            if (!result.ClientFullyConsumed)
            {
                result.ClientRemaining = new DeleteComponent { Count = clientDelete3.Count - serverDelete2.Count };
            }
            
            if (!result.ServerFullyConsumed)
            {
                result.ServerRemaining = new DeleteComponent { Count = serverDelete2.Count - clientDelete3.Count };
            }
        }
        
        return result;
    }

    public async Task<bool> ApplyComponentsAsync(
        string markdownId, 
        IEnumerable<OperationComponent> components,
        CancellationToken cancellationToken = default)
    {
        int position = 0;
        
        foreach (var component in components)
        {
            bool success = true;
            
            switch (component)
            {
                case RetainComponent retain:
                    position += retain.Count;
                    break;
                    
                case InsertComponent insert:
                    success = await _markdownRepository.InsertContentAsync(
                        markdownId, position, insert.Text, cancellationToken);
                    position += insert.Text.Length;
                    break;
                    
                case DeleteComponent delete:
                    success = await _markdownRepository.DeleteContentAsync(
                        markdownId, position, delete.Count, cancellationToken);
                    break;
            }
            
            if (!success)
            {
                return false;
            }
        }
        
        return true;
    }
}
