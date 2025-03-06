using Doner.Features.WorkspaceFeature;

namespace Doner.Features.AuthFeature.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string Login { get; set; } = null!;
    public string? Email { get; set; }
    public byte[] PasswordHash { get; set; } = null!;
    public byte[] PasswordSalt { get; set; } = null!;
    public List<WorkspaceInvite> InvitedWorkspaces { get; set; } = null!;
}