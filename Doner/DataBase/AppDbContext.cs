using Doner.Features.AuthFeature.Entities;
using Doner.Features.WorkspaceFeature.Entities;
using Microsoft.EntityFrameworkCore;

namespace Doner.DataBase;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<WorkspaceInvite> WorkspaceInvites { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ChangeTracker.Entries<User>()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity).Iter(user =>
                WorkspaceInvites.RemoveRange(WorkspaceInvites.Where(uw => uw.UserId == user.Id))
            );
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ChangeTracker.Entries<User>()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity).Iter(user =>
                WorkspaceInvites.RemoveRange(WorkspaceInvites.Where(uw => uw.UserId == user.Id))
            );
        return base.SaveChangesAsync(cancellationToken);
    }
}