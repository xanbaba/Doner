namespace Contracts.V1.Responses;

public class WorkspaceResponse
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Styles { get; set; }
    
    public Guid OwnerId { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
}