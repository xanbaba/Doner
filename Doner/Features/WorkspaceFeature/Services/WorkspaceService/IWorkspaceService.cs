using Doner.Features.WorkspaceFeature.Entities;
using LanguageExt;
using LanguageExt.Common;

namespace Doner.Features.WorkspaceFeature.Services.WorkspaceService;

public interface IWorkspaceService
{
    Task<Result<IEnumerable<Workspace>>> GetByOwnerAsync(Guid ownerId);
    
    Task<Result<Workspace>> GetAsync(Guid id);

    Task<Result<Guid>> CreateAsync(Workspace workspace);
    
    Task<Result<Unit>> UpdateAsync(Workspace workspace);
    
    Task<Result<Unit>> RemoveAsync(Guid userId, Guid workspaceId);
    Task<Result<Unit>> RemoveAsync(Guid userId, Workspace workspace);
}