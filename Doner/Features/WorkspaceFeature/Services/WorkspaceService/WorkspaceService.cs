using System.Text;
using Contracts.V1.Requests;
using Doner.DataBase;
using Doner.Features.AuthFeature.Entities;
using Doner.Features.WorkspaceFeature.Entities;
using Doner.Features.WorkspaceFeature.Exceptions;
using Doner.Features.WorkspaceFeature.Repository;
using Doner.Features.WorkspaceFeature.Services.EmailService;
using Doner.Features.WorkspaceFeature.Services.InviteLinkService;
using FluentValidation;
using LanguageExt;
using LanguageExt.Common;

namespace Doner.Features.WorkspaceFeature.Services.WorkspaceService;

public class WorkspaceService(IWorkspaceRepository workspaceRepository, IInviteTokenService inviteTokenService, IEmailService emailService, AppDbContext dbContext): WorkspaceServiceBase
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

    public override async Task<Result<Unit>> InviteUserAsync(Guid workspaceId, Guid userId, string email)
    {
        var workspace = await workspaceRepository.GetAsync(workspaceId);

        if (workspace is null)
        {
            return new Result<Unit>(new WorkspaceNotFoundException());
        }
        
        if (workspace.OwnerId != userId)
        {
            return new Result<Unit>(new PermissionDeniedException());
        }
        
        var inviteAlreadyExists = workspace.Invitees.Any(wi => wi.User.Email == email && wi.WorkspaceId == workspaceId);

        if (inviteAlreadyExists)
        {
            return new Result<Unit>(new WorkspaceInviteAlreadyExistsException());
        }
        
        var userToInvite = dbContext.Users.SingleOrDefault(u => u.Email == email);

        if (userToInvite is null)
        {
            return new Result<Unit>(new UserNotFoundException());
        }
        
        var token = inviteTokenService.GenerateToken(workspace.Id, userToInvite.Id);
        var link = $"https://localhost:3000/api/v1/users/me/workspaces/accept/{token}";
        
        await emailService.SendEmailInviteAsync(email, userToInvite.FirstName, link);
        
        return Unit.Default;
    }

    public override async Task<Result<Unit>> AcceptInviteAsync(Guid userId, string token)
    {
        var decrypted = inviteTokenService.DecryptToken(token);

        if (decrypted is null)
        {
            return new Result<Unit>(new InvalidInviteTokenException());
        }

        var (invitedUserId, workspaceId) = decrypted.Value;

        if (invitedUserId != userId)
        {
            return new Result<Unit>(new UnableToAcceptInviteException());
        }
        
        var workspace = await workspaceRepository.GetAsync(workspaceId);

        if (workspace is null)
        {
            return new Result<Unit>(new WorkspaceNotFoundException());
        }

        dbContext.WorkspaceInvites.Add(new WorkspaceInvite()
        {
            UserId = invitedUserId,
            WorkspaceId = workspaceId
        });
        await dbContext.SaveChangesAsync();
        
        return Unit.Default;
    }
}