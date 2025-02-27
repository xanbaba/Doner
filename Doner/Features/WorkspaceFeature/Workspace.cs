using Doner.Features.AuthFeature;

namespace Doner.Features.WorkspaceFeature;

public class Workspace
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public List<User> Invitees { get; set; } = null!;
}