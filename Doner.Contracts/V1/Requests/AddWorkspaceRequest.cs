namespace Contracts.V1.Requests;

public class AddWorkspaceRequest
{
    public required string Name { get; set; }
    public string? Styles { get; set; }
    public string? Description { get; set; }
}