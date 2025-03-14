using Doner.Features.WorkspaceFeature.Entities;

namespace Doner.Features.WorkspaceFeature.Repository;

public interface IWorkspaceRepository
{
    Task<IEnumerable<Workspace>> GetAsync();
    Task<Workspace?> GetAsync(Guid id);

    Task<Guid> AddAsync(Workspace workspace);
    
    Task UpdateAsync(Guid id, Workspace workspace);
    Task UpdateAsync(Workspace workspace);
    
    Task RemoveAsync(Guid id);
    Task RemoveAsync(Workspace workspace);
}