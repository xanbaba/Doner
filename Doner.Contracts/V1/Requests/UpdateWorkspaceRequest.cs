namespace Contracts.V1.Requests;

public class UpdateWorkspaceRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}