using Doner.Features.WorkspaceFeature.Entities;

namespace Doner.Features.WorkspaceFeature.Repository;

public abstract class WorkspaceRepositoryBase: IWorkspaceRepository
{
    public abstract Task<IEnumerable<Workspace>> GetAsync();

    public abstract Task<Workspace?> GetAsync(Guid id);

    public abstract Task<Guid> AddAsync(Workspace workspace);

    public abstract Task UpdateAsync(Guid id, Workspace workspace);

    public Task UpdateAsync(Workspace workspace)
    {
        return UpdateAsync(workspace.Id, workspace);
    }
    public abstract Task RemoveAsync(Guid id);

    public Task RemoveAsync(Workspace workspace)
    {
        return RemoveAsync(workspace.Id);
    }
}