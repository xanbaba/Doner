using Doner.Features.WorkspaceFeature.Entities;
using Doner.Features.WorkspaceFeature.Exceptions;
using Doner.Features.WorkspaceFeature.Repository;
using Doner.Localizer;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Localization;

namespace Doner.Features.WorkspaceFeature.Services.WorkspaceService;

public class WorkspaceService(IWorkspaceRepository workspaceRepository, IStringLocalizer<Messages> localizer): WorkspaceServiceBase
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
            return new Result<Workspace>(new WorkspaceNotFoundException(localizer["WorkspaceNotFound"].Value));
        }
        
        return new Result<Workspace>(workspace);
    }

    public override async Task<Result<Guid>> CreateAsync(Workspace workspace)
    {
        if (string.IsNullOrWhiteSpace(workspace.Name))
        {
            return new Result<Guid>(new WorkspaceNameRequiredException(localizer["WorkspaceNameRequired"].Value));
        }

        if (await workspaceRepository.Exists(workspace.OwnerId, workspace.Name))
        {
            return new Result<Guid>(new WorkspaceAlreadyExistsException(localizer["WorkspaceAlreadyExists"].Value));
        }
            
        workspace.Id = Guid.CreateVersion7();
        
        await workspaceRepository.AddAsync(workspace);
        
        return new Result<Guid>(workspace.Id);
    }

    public override async Task<Result<Unit>> UpdateAsync(Guid userId, Workspace workspace)
    {

        if (!await workspaceRepository.Exists(workspace.Id))
        {
            return new Result<Unit>(new WorkspaceNotFoundException(localizer["WorkspaceNotFound"].Value));
        }
        
        if (workspace.OwnerId != userId)
        {
            return new Result<Unit>(new PermissionDeniedException(localizer["PermissionDenied"].Value));
        }
        
        await workspaceRepository.UpdateAsync(workspace);
        
        return new Result<Unit>();
    }
    
    public override async Task<Result<Unit>> RemoveAsync(Guid userId, Guid workspaceId)
    {
        var workspace = await workspaceRepository.GetAsync(workspaceId);
        
        if (workspace == null)
        {
            return new Result<Unit>(new WorkspaceNotFoundException(localizer["WorkspaceNotFound"].Value));
        }
        
        if (workspace.OwnerId != userId)
        {
            return new Result<Unit>(new PermissionDeniedException(localizer["PermissionDenied"].Value));
        }
        
        await workspaceRepository.RemoveAsync(workspaceId);

        return new Result<Unit>();
    }
}