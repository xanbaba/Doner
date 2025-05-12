namespace Doner.Features.WorkspaceFeature.Services.InviteLinkService;

public interface IInviteTokenService
{
    string GenerateToken(Guid workspaceId, Guid userId);
    (Guid workspaceId, Guid userId)? DecryptToken(string link);
}