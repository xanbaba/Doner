using Doner.Features.WorkspaceFeature.Entities;
using Doner.Features.WorkspaceFeature.Exceptions;
using Doner.Features.WorkspaceFeature.Repository;
using Doner.Features.WorkspaceFeature.Services.WorkspaceService;
using FluentAssertions;
using Moq;

namespace Doner.Tests;

public class WorkspaceServiceTests
{
    private readonly Mock<IWorkspaceRepository> _workspaceRepositoryMock;
    private readonly WorkspaceService _workspaceService;

    public WorkspaceServiceTests()
    {
        _workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
        _workspaceService = new WorkspaceService(_workspaceRepositoryMock.Object, null!, null!, null!);
    }

    [Fact]
    public async Task GetByOwnerAsync_ShouldReturnWorkspaces_WhenOwnerExists()
    {
        var ownerId = Guid.NewGuid();
        var workspaces = new List<Workspace>
        {
            new Workspace { Id = Guid.NewGuid(), Name = "Workspace 1", OwnerId = ownerId },
            new Workspace { Id = Guid.NewGuid(), Name = "Workspace 2", OwnerId = ownerId }
        };
        _workspaceRepositoryMock.Setup(r => r.GetByOwnerAsync(ownerId)).ReturnsAsync(workspaces);

        var result = await _workspaceService.GetByOwnerAsync(ownerId);

        result.IsSuccess.Should().BeTrue();
        result.IfSucc(value => value.Should().BeEquivalentTo(workspaces));
    }

    [Fact]
    public async Task GetAsync_ShouldReturnWorkspace_WhenWorkspaceExistsAndUserIsOwner()
    {
        var userId = Guid.NewGuid();
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", OwnerId = userId };
        _workspaceRepositoryMock.Setup(r => r.GetAsync(workspace.Id)).ReturnsAsync(workspace);

        var result = await _workspaceService.GetAsync(workspace.Id, userId);

        result.IsSuccess.Should().BeTrue();
        result.IfSucc(value => value.Should().Be(workspace));
    }

    [Fact]
    public async Task GetAsync_ShouldReturnPermissionDeniedException_WhenUserIsNotOwner()
    {
        var userId = Guid.NewGuid();
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", OwnerId = Guid.NewGuid() };
        _workspaceRepositoryMock.Setup(r => r.GetAsync(workspace.Id)).ReturnsAsync(workspace);

        var result = await _workspaceService.GetAsync(workspace.Id, userId);

        result.IsFaulted.Should().BeTrue();
        result.IfFail(e => e.Should().BeOfType<PermissionDeniedException>());
    }

    [Fact]
    public async Task GetAsync_ShouldReturnWorkspaceNotFoundException_WhenWorkspaceDoesNotExist()
    {
        _workspaceRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Guid>())).ReturnsAsync((Workspace?)null);

        var result = await _workspaceService.GetAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsFaulted.Should().BeTrue();
        result.IfFail(e => e.Should().BeOfType<WorkspaceNotFoundException>());
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnWorkspaceId_WhenWorkspaceIsCreated()
    {
        var workspace = new Workspace { Name = "New Workspace", OwnerId = Guid.NewGuid() };
        _workspaceRepositoryMock.Setup(r => r.Exists(workspace.OwnerId, workspace.Name)).ReturnsAsync(false);
        _workspaceRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Workspace>())).ReturnsAsync(workspace.Id);

        var result = await _workspaceService.CreateAsync(workspace);

        result.IsSuccess.Should().BeTrue();
        result.IfSucc(value => value.Should().Be(workspace.Id));
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnWorkspaceAlreadyExistsException_WhenWorkspaceWithSameNameExists()
    {
        var workspace = new Workspace { Name = "Existing Workspace", OwnerId = Guid.NewGuid() };
        _workspaceRepositoryMock.Setup(r => r.Exists(workspace.OwnerId, workspace.Name)).ReturnsAsync(true);

        var result = await _workspaceService.CreateAsync(workspace);

        result.IsFaulted.Should().BeTrue();
        result.IfFail(e => e.Should().BeOfType<WorkspaceAlreadyExistsException>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateWorkspace_WhenWorkspaceExistsAndUserIsOwner()
    {
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Updated Workspace", OwnerId = Guid.NewGuid() };
        _workspaceRepositoryMock.Setup(r => r.GetAsync(workspace.Id)).ReturnsAsync(workspace);
        _workspaceRepositoryMock.Setup(r => r.Exists(workspace.OwnerId, workspace.Name)).ReturnsAsync(false);

        var result = await _workspaceService.UpdateAsync(workspace);

        result.IsSuccess.Should().BeTrue();
        _workspaceRepositoryMock.Verify(r => r.UpdateAsync(workspace), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnPermissionDeniedException_WhenUserIsNotOwner()
    {
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", OwnerId = Guid.NewGuid() };
        var existingWorkspace = new Workspace { Id = workspace.Id, Name = "Existing Workspace", OwnerId = Guid.NewGuid() };
        _workspaceRepositoryMock.Setup(r => r.GetAsync(workspace.Id)).ReturnsAsync(existingWorkspace);

        var result = await _workspaceService.UpdateAsync(workspace);

        result.IsFaulted.Should().BeTrue();
        result.IfFail(e => e.Should().BeOfType<PermissionDeniedException>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnWorkspaceNotFoundException_WhenWorkspaceDoesNotExist()
    {
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Nonexistent Workspace", OwnerId = Guid.NewGuid() };
        _workspaceRepositoryMock.Setup(r => r.GetAsync(workspace.Id)).ReturnsAsync((Workspace?)null);

        var result = await _workspaceService.UpdateAsync(workspace);

        result.IsFaulted.Should().BeTrue();
        result.IfFail(e => e.Should().BeOfType<WorkspaceNotFoundException>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveWorkspace_WhenWorkspaceExistsAndUserIsOwner()
    {
        var userId = Guid.NewGuid();
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", OwnerId = userId };
        _workspaceRepositoryMock.Setup(r => r.GetAsync(workspace.Id)).ReturnsAsync(workspace);

        var result = await _workspaceService.RemoveAsync(workspace.Id, userId);

        result.IsSuccess.Should().BeTrue();
        _workspaceRepositoryMock.Verify(r => r.RemoveAsync(workspace.Id), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnPermissionDeniedException_WhenUserIsNotOwner()
    {
        var userId = Guid.NewGuid();
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", OwnerId = Guid.NewGuid() };
        _workspaceRepositoryMock.Setup(r => r.GetAsync(workspace.Id)).ReturnsAsync(workspace);

        var result = await _workspaceService.RemoveAsync(workspace.Id, userId);

        result.IsFaulted.Should().BeTrue();
        result.IfFail(e => e.Should().BeOfType<PermissionDeniedException>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnWorkspaceNotFoundException_WhenWorkspaceDoesNotExist()
    {
        _workspaceRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Guid>())).ReturnsAsync((Workspace?)null);

        var result = await _workspaceService.RemoveAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsFaulted.Should().BeTrue();
        result.IfFail(e => e.Should().BeOfType<WorkspaceNotFoundException>());
    }
}