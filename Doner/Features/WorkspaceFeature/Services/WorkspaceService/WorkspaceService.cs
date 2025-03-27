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

    public override async Task<Result<Workspace>> GetAsync(Guid id)
    {
        var workspace = await workspaceRepository.GetAsync(id);
        
        if (workspace is null)
        {
            return new Result<Workspace>(new WorkspaceNotFoundException());
        }
        
        return new Result<Workspace>(workspace);
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
        
        return new Result<Guid>(workspace.Id);
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
        
        return new Result<Unit>();
    }
    
    public override async Task<Result<Unit>> RemoveAsync(Guid userId, Guid workspaceId)
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

        return new Result<Unit>();
    }
}