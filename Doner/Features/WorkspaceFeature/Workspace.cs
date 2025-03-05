using Doner.Features.AuthFeature.Entities;
using Doner.Features.ReelsFeature;

namespace Doner.Features.WorkspaceFeature;

public class Workspace
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public List<WorkspaceInvite> Invitees { get; set; } = null!;
    
    public List<Reel> Reels { get; set; } = null!;
}