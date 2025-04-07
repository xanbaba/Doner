namespace Contracts.V1.Responses;

public class ReelResponse
{
    public required Guid Id { get; set; }
    
    public required string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    public Guid WorkspaceId { get; set; }

    public Guid OwnerId { get; set; }
}