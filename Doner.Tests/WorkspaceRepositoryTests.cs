using Doner.DataBase;
using Doner.Features.WorkspaceFeature.Entities;
using Doner.Features.WorkspaceFeature.Repository;
using FluentAssertions;

namespace Doner.Tests;

public class WorkspaceRepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly WorkspaceRepository _repository;

    public WorkspaceRepositoryTests()
    {
        var dbContextFactory = new AppDbContextFactory();
        _dbContext = dbContextFactory.CreateDbContext();
        _repository = new WorkspaceRepository(dbContextFactory);
    }

    [Fact]
    public async Task GetByOwnerAsync_ShouldReturnWorkspaces_WhenOwnerExists()
    {
        var ownerId = Guid.NewGuid();
        var workspaces = new List<Workspace>
        {
            new() { Id = Guid.NewGuid(), Name = "Workspace 1", OwnerId = ownerId },
            new() { Id = Guid.NewGuid(), Name = "Workspace 2", OwnerId = ownerId }
        };
        await _dbContext.Workspaces.AddRangeAsync(workspaces);
        await _dbContext.SaveChangesAsync();

        var result = (await _repository.GetByOwnerAsync(ownerId)).ToList();
        _dbContext.ChangeTracker.Clear();

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(workspaces);
    }

    [Fact]
    public async Task GetByOwnerAsync_ShouldReturnEmpty_WhenOwnerHasNoWorkspaces()
    {
        var ownerId = Guid.NewGuid();
        var workspaces = new List<Workspace>
        {
            new() { Id = Guid.NewGuid(), Name = "Workspace 1", OwnerId = ownerId },
            new() { Id = Guid.NewGuid(), Name = "Workspace 2", OwnerId = ownerId }
        };
        await _dbContext.Workspaces.AddRangeAsync(workspaces);
        await _dbContext.SaveChangesAsync();
        
        var result = (await _repository.GetByOwnerAsync(Guid.NewGuid())).ToList();
        _dbContext.ChangeTracker.Clear();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnWorkspace_WhenWorkspaceExists()
    {
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Test Workspace", OwnerId = Guid.NewGuid() };
        await _dbContext.Workspaces.AddAsync(workspace);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAsync(workspace.Id);
        _dbContext.ChangeTracker.Clear();

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(workspace);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenWorkspaceDoesNotExist()
    {
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Test Workspace", OwnerId = Guid.NewGuid() };
        await _dbContext.Workspaces.AddAsync(workspace);
        await _dbContext.SaveChangesAsync();
        
        var result = await _repository.GetAsync(Guid.NewGuid());
        _dbContext.ChangeTracker.Clear();

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldInsertWorkspace()
    {
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "New Workspace", OwnerId = Guid.NewGuid() };

        var result = await _repository.AddAsync(workspace);
        _dbContext.ChangeTracker.Clear();

        result.Should().Be(workspace.Id);
        var inserted = await _dbContext.Workspaces.FindAsync(workspace.Id);
        inserted.Should().NotBeNull();
        inserted.Should().BeEquivalentTo(workspace);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateWorkspace_WhenWorkspaceExists()
    {
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Old Name", Description = "Old Description", OwnerId = Guid.NewGuid() };
        await _dbContext.Workspaces.AddAsync(workspace);
        await _dbContext.SaveChangesAsync();

        workspace.Name = "Updated Name";
        workspace.Description = "Updated Description";
        await _repository.UpdateAsync(workspace.Id, workspace);
        _dbContext.ChangeTracker.Clear();

        var updated = await _dbContext.Workspaces.FindAsync(workspace.Id);
        updated.Should().NotBeNull();
        updated.Name.Should().Be("Updated Name");
        updated.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task UpdateAsync_ShouldDoNothing_WhenWorkspaceDoesNotExist()
    {
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Nonexistent Workspace", OwnerId = Guid.NewGuid() };

        await _repository.UpdateAsync(workspace.Id, workspace);

        var result = await _dbContext.Workspaces.FindAsync(workspace.Id);
        _dbContext.ChangeTracker.Clear();
        
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteWorkspace_WhenWorkspaceExists()
    {
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace to Delete", OwnerId = Guid.NewGuid() };
        await _dbContext.Workspaces.AddAsync(workspace);
        await _dbContext.SaveChangesAsync();

        await _repository.RemoveAsync(workspace.Id);
        _dbContext.ChangeTracker.Clear();

        var deleted = await _dbContext.Workspaces.FindAsync(workspace.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_ShouldDoNothing_WhenWorkspaceDoesNotExist()
    {
        var workspaceId = Guid.NewGuid();

        await _repository.RemoveAsync(workspaceId);
        _dbContext.ChangeTracker.Clear();

        var result = await _dbContext.Workspaces.FindAsync(workspaceId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Exists_ShouldReturnTrue_WhenWorkspaceExists()
    {
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Existing Workspace", OwnerId = Guid.NewGuid() };
        await _dbContext.Workspaces.AddAsync(workspace);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.Exists(workspace.Id);
        _dbContext.ChangeTracker.Clear();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Exists_ShouldReturnFalse_WhenWorkspaceDoesNotExist()
    {
        var result = await _repository.Exists(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Exists_ShouldReturnTrue_WhenWorkspaceWithNameExistsForOwner()
    {
        var ownerId = Guid.NewGuid();
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Unique Name", OwnerId = ownerId };
        await _dbContext.Workspaces.AddAsync(workspace);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.Exists(ownerId, "Unique Name");
        _dbContext.ChangeTracker.Clear();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Exists_ShouldReturnFalse_WhenWorkspaceWithNameDoesNotExistForOwner()
    {
        var result = await _repository.Exists(Guid.NewGuid(), "Nonexistent Name");

        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}