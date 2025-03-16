using Doner.Features.AuthFeature.Entities;

namespace Doner.Features.WorkspaceFeature.Entities;

public class Workspace
{
    public Guid Id { get; init; }
    
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    public Guid OwnerId { get; init; }
    public User Owner { get; init; } = null!;

    public List<WorkspaceInvite> Invitees { get; init; } = null!;
}