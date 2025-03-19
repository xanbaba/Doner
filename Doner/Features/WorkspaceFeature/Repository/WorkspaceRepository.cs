using Doner.DataBase;
using Doner.Features.WorkspaceFeature.Entities;
using Microsoft.EntityFrameworkCore;

namespace Doner.Features.WorkspaceFeature.Repository;

public class WorkspaceRepository(IDbContextFactory<AppDbContext> dbContextFactory): WorkspaceRepositoryBase
{
    public override async Task<IEnumerable<Workspace>> GetByOwnerAsync(Guid ownerId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        return context.Workspaces
            .Where(w => w.OwnerId == ownerId);
    }

    public override async Task<Workspace?> GetAsync(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        return context.Workspaces.FirstOrDefault(w => w.Id == id);
    }

    public override async Task<Guid> AddAsync(Workspace workspace)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        var workspaceId = (await context.Workspaces.AddAsync(workspace)).Entity.Id;
        
        await context.SaveChangesAsync();
        
        return workspaceId;
    }

    public override async Task UpdateAsync(Guid id, Workspace workspace)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        var entity = await context.Workspaces.FindAsync(id);
        if (entity == null)
        {
            return;
        }

        context.Entry(entity).CurrentValues.SetValues(workspace);
        
        await context.SaveChangesAsync();
    }

    public override async Task RemoveAsync(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        context.Workspaces.Remove(new Workspace { Id = id });
        
        await context.SaveChangesAsync();
    }
    
    public override async Task<bool> Exists(Guid workspaceId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        return context.Workspaces
            .Any(w => w.Id == workspaceId);
    }
    
    public override async Task<bool> Exists(Guid ownerId, string workspaceName)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        return context.Workspaces.Any(w => w.OwnerId == ownerId && w.Name == workspaceName);
    }
}