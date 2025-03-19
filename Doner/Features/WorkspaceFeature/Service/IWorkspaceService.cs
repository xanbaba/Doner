using Doner.Features.WorkspaceFeature.Entities;
using LanguageExt;
using LanguageExt.Common;
using Sprache;

namespace Doner.Features.WorkspaceFeature.Service;

public interface IWorkspaceService
{
    Task<Result<IEnumerable<Workspace>>> GetByOwnerAsync(Guid ownerId);
    
    Task<Result<Workspace>> GetAsync(Guid id);

    Task<Result<Guid>> CreateAsync(Workspace workspace);
    
    Task<Result<Unit>> UpdateAsync(Guid userId ,Workspace workspace);
    Task<Result<Unit>> UpdateAsync(Guid workspaceId, Guid userId ,Workspace workspace);
    
    Task<Result<Unit>> RemoveAsync(Guid userId, Guid workspaceId);
    Task<Result<Unit>> RemoveAsync(Guid userId, Workspace workspace);
}