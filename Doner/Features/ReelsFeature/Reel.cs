using Doner.Features.WorkspaceFeature;

namespace Doner.Features.ReelsFeature;

public class Reel
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
}