using Doner.Features.WorkspaceFeature.Entities;
using Doner.Features.WorkspaceFeature.Exceptions;
using Doner.Features.WorkspaceFeature.Repository;
using LanguageExt;
using LanguageExt.Common;

namespace Doner.Features.WorkspaceFeature.Services.WorkspaceService;

public class WorkspaceService(IWorkspaceRepository workspaceRepository): WorkspaceServiceBase
{
    public override async Task<Result<IEnumerable<Workspace>>> GetByOwnerAsync(Guid ownerId)
    {
        var workspaces = await workspaceRepository.GetByOwnerAsync(ownerId);
        
        return new Result<IEnumerable<Workspace>>(workspaces);
    }

    public override async Task<Result<Workspace>> GetAsync(Guid id, Guid userId)
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

    public override async Task<Result<Guid>> CreateAsync(Workspace workspace)
    {
        if (string.IsNullOrWhiteSpace(workspace.Name))
        {
            return new Result<Guid>(new WorkspaceNameRequiredException());
        }

        if (await workspaceRepository.Exists(workspace.OwnerId, workspace.Name))
        {
            return new Result<Guid>(new WorkspaceAlreadyExistsException());
        }
            
        workspace.Id = Guid.CreateVersion7();
        
        await workspaceRepository.AddAsync(workspace);
        
        return workspace.Id;
    }

    public override async Task<Result<Unit>> UpdateAsync(Workspace workspace)
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
    
    public override async Task<Result<Unit>> RemoveAsync(Guid workspaceId, Guid userId)
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
}