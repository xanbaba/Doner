using Doner.DataBase;
using Doner.Features.WorkspaceFeature.Entities;
using Doner.Features.WorkspaceFeature.Exceptions;
using Doner.Features.WorkspaceFeature.Repository;
using Doner.Features.WorkspaceFeature.Services.EmailService;
using Doner.Features.WorkspaceFeature.Services.InviteLinkService;
using LanguageExt;
using LanguageExt.Common;

namespace Doner.Features.WorkspaceFeature.Services.WorkspaceService;

public class WorkspaceService(IWorkspaceRepository workspaceRepository, IInviteTokenService inviteTokenService, IEmailService emailService, AppDbContext dbContext, IHttpContextAccessor httpContextAccessor): IWorkspaceService
{
    public async Task<Result<IEnumerable<Workspace>>> GetByOwnerAsync(Guid userId)
    {
        var workspaces = await workspaceRepository.GetWorkspaces(userId);
        
        return new Result<IEnumerable<Workspace>>(workspaces);
    }

    public async Task<Result<Workspace>> GetAsync(Guid id, Guid userId)
    {
        var workspace = await workspaceRepository.GetAsync(id);
        
        if (workspace == null)
        {
            return new Result<Workspace>(new WorkspaceNotFoundException());
        }

        if (!await workspaceRepository.IsUserInWorkspaceAsync(workspace.Id, userId))
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
    
    public async Task<Result<Unit>> InviteUserAsync(Guid workspaceId, Guid userId, string email)
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

        var invitees = dbContext.WorkspaceInvites.Where(wi => wi.WorkspaceId == workspace.Id);
        var inviteAlreadyExists = invitees.Any(wi => wi.User.Email == email && wi.WorkspaceId == workspaceId);
        
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
        
        var request = httpContextAccessor.HttpContext?.Request;
        var baseUrl = $"{request?.Scheme}://{request?.Host}";
        var link = $"{baseUrl}/api/v1/users/me/workspaces/accept/{token}";
        
        await emailService.SendEmailInviteAsync(email, userToInvite.Username, link);
        
        return Unit.Default;
    }

    public async Task<Result<Unit>> AcceptInviteAsync(string token)
    {
        var decrypted = inviteTokenService.DecryptToken(token);

        if (decrypted is null)
        {
            return new Result<Unit>(new InvalidInviteTokenException());
        }

        var (workspaceId, invitedUserId) = decrypted.Value;
        
        var workspace = await workspaceRepository.GetAsync(workspaceId);

        if (workspace is null)
        {
            return new Result<Unit>(new WorkspaceNotFoundException());
        }
        
        var invitees = dbContext.WorkspaceInvites.Where(wi => wi.WorkspaceId == workspace.Id);
        var inviteAlreadyAccepted = invitees.Any(wi => wi.UserId == invitedUserId && wi.WorkspaceId == workspaceId);
        
        if (inviteAlreadyAccepted)
        {
            return new Result<Unit>(new InviteAlreadyAcceptedException());
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