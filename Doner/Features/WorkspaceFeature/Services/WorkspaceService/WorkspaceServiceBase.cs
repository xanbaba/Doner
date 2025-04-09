using Doner.Features.WorkspaceFeature.Entities;
using LanguageExt;
using LanguageExt.Common;

namespace Doner.Features.WorkspaceFeature.Services.WorkspaceService;

public abstract class WorkspaceServiceBase: IWorkspaceService
{
    public abstract Task<Result<IEnumerable<Workspace>>> GetByOwnerAsync(Guid ownerId);

    public abstract Task<Result<Workspace>> GetAsync(Guid id, Guid userId);

    public abstract Task<Result<Guid>> CreateAsync(Workspace workspace);

    public abstract Task<Result<Unit>> UpdateAsync(Workspace workspace);

    public abstract Task<Result<Unit>> RemoveAsync(Guid workspaceId, Guid userId);

    public async Task<Result<Unit>> RemoveAsync(Workspace workspace, Guid userId)
    {
        return await RemoveAsync(userId, workspace.Id);
    }
}