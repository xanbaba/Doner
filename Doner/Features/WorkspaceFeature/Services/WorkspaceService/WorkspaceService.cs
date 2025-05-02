using Doner.DataBase;
using Doner.Features.WorkspaceFeature.Entities;
using Doner.Features.WorkspaceFeature.Exceptions;
using Doner.Features.WorkspaceFeature.Repository;
using LanguageExt;
using LanguageExt.Common;

namespace Doner.Features.WorkspaceFeature.Services.WorkspaceService;

public class WorkspaceService(IWorkspaceRepository workspaceRepository, AppDbContext dbContext): IWorkspaceService
{
    public async Task<Result<IEnumerable<Workspace>>> GetByOwnerAsync(Guid ownerId)
    {
        var workspaces = await workspaceRepository.GetByOwnerAsync(ownerId);
        
        return new Result<IEnumerable<Workspace>>(workspaces);
    }

    public async Task<Result<Workspace>> GetAsync(Guid id, Guid userId)
    {
        var workspace = await workspaceRepository.GetAsync(id);
        
        if (workspace == null)
        {
            return new Result<Workspace>(new WorkspaceNotFoundException());
        }

        if (workspace.OwnerId != userId)
        {
            return new Result<Workspace>(new PermissionDeniedException());
        }
        
        return workspace;
    }

    public async Task<Result<Guid>> CreateAsync(Workspace workspace)
    {
        if (string.IsNullOrWhiteSpace(workspace.Name))
        {
            return new Result<Guid>(new WorkspaceNameRequiredException());
        }
        
        var owner = await dbContext.Users.FindAsync(workspace.OwnerId);

        if (owner is null || owner.Id != workspace.OwnerId)
        {
            return new Result<Guid>(new PermissionDeniedException());
        }

        if (await workspaceRepository.Exists(workspace.OwnerId, workspace.Name))
        {
            return new Result<Guid>(new WorkspaceAlreadyExistsException());
        }
        
        workspace.CreatedAtUtc = DateTime.UtcNow;
        await workspaceRepository.AddAsync(workspace);
        
        return workspace.Id;
    }

    public async Task<Result<Unit>> UpdateAsync(Workspace workspace)
    {
        var existingWorkspace = await workspaceRepository.GetAsync(workspace.Id);

        if (existingWorkspace is null)
        {
            return new Result<Unit>(new WorkspaceNotFoundException());
        }
        
        if (existingWorkspace.OwnerId != workspace.OwnerId)
        {
            return new Result<Unit>(new PermissionDeniedException());
        }
        
        if (await workspaceRepository.Exists(workspace.OwnerId, workspace.Name))
        {
            return new Result<Unit>(new WorkspaceAlreadyExistsException());
        }
        
        await workspaceRepository.UpdateAsync(workspace);
        
        return Unit.Default;
    }
    
    public async Task<Result<Unit>> RemoveAsync(Guid workspaceId, Guid userId)
    {
        var workspace = await workspaceRepository.GetAsync(workspaceId);
        
        if (workspace == null)
        {
            return new Result<Unit>(new WorkspaceNotFoundException());
        }
        
        if (workspace.OwnerId != userId)
        {
            return new Result<Unit>(new PermissionDeniedException());
        }
        
        await workspaceRepository.RemoveAsync(workspaceId);

        return Unit.Default;
    }

    public Task<Result<Unit>> RemoveAsync(Workspace workspace, Guid userId)
    {
        return RemoveAsync(workspace.Id, userId);
    }
}