using Doner.Features.AuthFeature.Entities;
using Doner.Features.ReelsFeature;
using Doner.Features.WorkspaceFeature;
using Microsoft.EntityFrameworkCore;

namespace Doner.DataBase;

public class AppDbContext(IConfiguration configuration): DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<Reel> Reels { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseSqlServer(connectionString);
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}