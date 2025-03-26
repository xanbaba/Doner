using Doner.Features.WorkspaceFeature.Entities;
using LanguageExt;
using LanguageExt.Common;

namespace Doner.Features.WorkspaceFeature.Services.WorkspaceService;

public abstract class WorkspaceServiceBase: IWorkspaceService
{
    public abstract Task<Result<IEnumerable<Workspace>>> GetByOwnerAsync(Guid ownerId);

    public abstract Task<Result<Workspace>> GetAsync(Guid id);

    public abstract Task<Result<Guid>> CreateAsync(Workspace workspace);

    public abstract Task<Result<Unit>> UpdateAsync(Guid userId ,Workspace workspace);

    public Task<Result<Unit>> UpdateAsync(Guid workspaceId, Guid userId ,Workspace workspace)
    {
        workspace.Id = workspaceId;
        return UpdateAsync(userId, workspace);
    }

    public abstract Task<Result<Unit>> RemoveAsync(Guid userId, Guid workspaceId);

    public async Task<Result<Unit>> RemoveAsync(Guid userId, Workspace workspace)
    {
        return await RemoveAsync(userId, workspace.Id);
    }
}