using Doner.Features.WorkspaceFeature.Entities;
using LanguageExt;
using LanguageExt.Common;
using Sprache;

namespace Doner.Features.WorkspaceFeature.Services.WorkspaceService;

public interface IWorkspaceService
{
    Task<Result<IEnumerable<Workspace>>> GetByOwnerAsync(Guid ownerId);
    
    Task<Result<Workspace>> GetAsync(Guid id, Guid userId);

    Task<Result<Guid>> CreateAsync(Workspace workspace);
    
    Task<Result<Unit>> UpdateAsync(Workspace workspace);
    
    Task<Result<Unit>> RemoveAsync(Guid workspaceId, Guid userId);
    Task<Result<Unit>> RemoveAsync(Workspace workspace, Guid userId);
    Task<Result<Unit>> InviteUserAsync(Guid workspaceId, Guid userId, string email);
    Task<Result<Unit>> AcceptInviteAsync(string token);
}