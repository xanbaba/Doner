using Doner.Features.WorkspaceFeature.Entities;

namespace Doner.Features.AuthFeature.Entities;

public class User
{
    public Guid Id { get; init; }
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string Login { get; init; } = null!;
    public string? Email { get; set; }
    public byte[] PasswordHash { get; set; } = null!;
    public byte[] PasswordSalt { get; set; } = null!;
    public List<WorkspaceInvite> InvitedWorkspaces { get; init; } = null!;
}