using Doner.Features.AuthFeature.Entities;

namespace Doner.Features.WorkspaceFeature.Entities;

public class Workspace
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    public string? Styles { get; set; }
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    
    public DateTime CreatedAtUtc { get; set; }

    public List<WorkspaceInvite> Invitees { get; set; } = null!;
}