using Doner.DataBase;
using Doner.Features.WorkspaceFeature.Entities;
using Microsoft.EntityFrameworkCore;

namespace Doner.Features.WorkspaceFeature.Repository;

public class WorkspaceRepository(IDbContextFactory<AppDbContext> dbContextFactory): IWorkspaceRepository
{
    public async Task<IEnumerable<Workspace>> GetByOwnerAsync(Guid ownerId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        return context.Workspaces
            .Where(w => w.OwnerId == ownerId).OrderBy(x => x.CreatedAtUtc).ToArray();
    }

    public async Task<Workspace?> GetAsync(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        return context.Workspaces.FirstOrDefault(w => w.Id == id);
    }

    public async Task<Guid> AddAsync(Workspace workspace)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        var workspaceId = (await context.Workspaces.AddAsync(workspace)).Entity.Id;
        
        await context.SaveChangesAsync();
        
        return workspaceId;
    }

    public async Task UpdateAsync(Guid id, Workspace workspace)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        var entity = await context.Workspaces.FindAsync(id);
        if (entity == null)
        {
            return;
        }

        entity.Name = workspace.Name;
        entity.Description = workspace.Description;
        
        await context.SaveChangesAsync();
    }

    public Task UpdateAsync(Workspace workspace)
    {
        return UpdateAsync(workspace.Id, workspace);
    }

    public async Task RemoveAsync(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var workspace = context.Workspaces.FirstOrDefault(x => x.Id == id);
        if (workspace is null)
        {
            return;
        }
        context.Remove(workspace);
        
        await context.SaveChangesAsync();
    }

    public Task RemoveAsync(Workspace workspace)
    {
        return RemoveAsync(workspace.Id);
    }

    public async Task<bool> Exists(Guid workspaceId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        return context.Workspaces
            .Any(w => w.Id == workspaceId);
    }
    
    public async Task<bool> Exists(Guid ownerId, string workspaceName)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        return context.Workspaces.Any(w => w.OwnerId == ownerId && w.Name == workspaceName);
    }
    
    public async Task<bool> IsUserInWorkspaceAsync(Guid workspaceId, Guid userId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        // Check if user is the owner of the workspace
        if (context.Workspaces.Any(w => w.Id == workspaceId && w.OwnerId == userId))
        {
            return true;
        }
        
        // Check if user is invited to the workspace through workspace members
        return context.WorkspaceInvites.Any(m => m.WorkspaceId == workspaceId && m.UserId == userId);
    }
}