namespace Contracts.V1.Requests;

public class AddReelElementRequest
{
    public required AddType AddType { get; set; }
    /// <summary>
    /// The ID of the element after which the new element should be inserted.
    /// Present only if AddType is InsertAfter.
    /// </summary>
    public Guid? InsertAfterId { get; set; }
    
    public string? Data { get; set; }
}