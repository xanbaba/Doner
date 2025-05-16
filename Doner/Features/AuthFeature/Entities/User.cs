using Doner.Features.WorkspaceFeature.Entities;

namespace Doner.Features.AuthFeature.Entities;

public class User
{
    public Guid Id { get; init; }
    public string Username { get; set; } = null!;
    public string Login { get; init; } = null!;
    public string Email { get; set; } = null!;
    public byte[] PasswordHash { get; set; } = null!;
    public byte[] PasswordSalt { get; set; } = null!;
    public List<WorkspaceInvite> InvitedWorkspaces { get; init; } = null!;
}