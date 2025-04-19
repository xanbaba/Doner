using Doner.Features.AuthFeature.Entities;
using Doner.Features.WorkspaceFeature.Entities;

namespace Doner.Features.WorkspaceFeature.Repository;

public interface IWorkspaceRepository
{
    Task<IEnumerable<Workspace>> GetByOwnerAsync(Guid ownerId);
    Task<Workspace?> GetAsync(Guid id);

    Task<Guid> AddAsync(Workspace workspace);
    
    Task UpdateAsync(Guid id, Workspace workspace);
    Task UpdateAsync(Workspace workspace);
    
    Task RemoveAsync(Guid id);
    Task RemoveAsync(Workspace workspace);
    Task<bool> Exists(Guid workspaceId);
    
    Task<bool> Exists(Guid ownerId, string workspaceName);
}