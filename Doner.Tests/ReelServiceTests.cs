using Doner.Features.ReelsFeature;
using Doner.Features.ReelsFeature.Repository;
using Doner.Features.ReelsFeature.Services;
using Doner.Features.WorkspaceFeature.Entities;
using Doner.Features.WorkspaceFeature.Exceptions;
using Doner.Features.WorkspaceFeature.Services.WorkspaceService;
using FluentAssertions;
using FluentValidation;
using LanguageExt.Common;
using Moq;
using SearchOption = Contracts.V1.SearchOption;

namespace Doner.Tests;

public class ReelServiceTests
{
    private readonly Mock<IReelRepository> _reelRepositoryMock;
    private readonly Mock<IWorkspaceService> _workspaceServiceMock;
    private readonly ReelService _reelService;

    public ReelServiceTests()
    {
        _reelRepositoryMock = new Mock<IReelRepository>();
        _workspaceServiceMock = new Mock<IWorkspaceService>();
        IValidator<Reel> reelValidator = new InlineValidator<Reel>
        {
            v => v.RuleFor(r => r.Name).NotEmpty(),
            v => v.RuleFor(r => r.WorkspaceId).NotEmpty()
        };

        _reelService = new ReelService(_reelRepositoryMock.Object, reelValidator,
            _workspaceServiceMock.Object);
    }

    [Fact]
    public async Task AddAsync_ShouldCallRepositoryAdd()
    {
        var reel = CreateTestReel();

        await _reelService.AddAsync(reel);

        _reelRepositoryMock.Verify(r => r.AddAsync(reel, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReel_WhenUserIsOwner()
    {
        var reel = CreateTestReel();
        _reelRepositoryMock.Setup(r => r.GetByIdAsync(reel.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reel);

        var result = await _reelService.GetByIdAsync(reel.Id, reel.OwnerId);

        result.Should().Be(reel);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        var reel = CreateTestReel();
        _reelRepositoryMock.Setup(r => r.GetByIdAsync(reel.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reel);

        Func<Task> act = async () => await _reelService.GetByIdAsync(reel.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateReel_WhenUserIsOwner()
    {
        var reel = CreateTestReel();
        _reelRepositoryMock.Setup(r => r.GetByIdAsync(reel.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reel);
        _reelRepositoryMock.Setup(r => r.UpdateAsync(reel, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _reelService.UpdateAsync(reel);

        result.Should().BeTrue();
        _reelRepositoryMock.Verify(r => r.UpdateAsync(reel, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        var id = Guid.NewGuid();
        var reel = CreateTestReel(id);
        var reel2 = CreateTestReel(id);
        _reelRepositoryMock.Setup(r => r.GetByIdAsync(reel.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reel);

        Func<Task> act = async () => await _reelService.UpdateAsync(reel2);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteReel_WhenUserIsOwner()
    {
        var reel = CreateTestReel();
        _reelRepositoryMock.Setup(r => r.GetByIdAsync(reel.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reel);
        _reelRepositoryMock.Setup(r => r.DeleteAsync(reel.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _reelService.DeleteAsync(reel.Id, reel.OwnerId);

        result.Should().BeTrue();
        _reelRepositoryMock.Verify(r => r.DeleteAsync(reel.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        var reel = CreateTestReel();
        _reelRepositoryMock.Setup(r => r.GetByIdAsync(reel.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reel);

        Func<Task> act = async () => await _reelService.DeleteAsync(reel.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetByWorkspaceAsync_ShouldReturnReels_WhenUserIsWorkspaceOwner()
    {
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var reels = new List<Reel> { CreateTestReel(workspaceId: workspaceId, ownerId: userId) };
        _workspaceServiceMock.Setup(w => w.GetAsync(workspaceId, userId))
            .ReturnsAsync(new Result<Workspace>(new Workspace { OwnerId = userId }));
        _reelRepositoryMock.Setup(r => r.GetByWorkspaceAsync(workspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reels);

        var result = await _reelService.GetByWorkspaceAsync(workspaceId, userId);

        result.Should().BeEquivalentTo(reels);
    }

    [Fact]
    public async Task GetByWorkspaceAsync_ShouldThrowWorkspaceNotFoundException_WhenWorkspaceDoesNotExist()
    {
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _workspaceServiceMock.Setup(w => w.GetAsync(workspaceId, userId))
            .ReturnsAsync(new Result<Workspace>(new WorkspaceNotFoundException()));

        Func<Task> act = async () => await _reelService.GetByWorkspaceAsync(workspaceId, userId);

        await act.Should().ThrowAsync<WorkspaceNotFoundException>();
    }

    [Fact]
    public async Task GetByWorkspaceAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotWorkspaceOwner()
    {
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _workspaceServiceMock.Setup(w => w.GetAsync(workspaceId, userId))
            .ReturnsAsync(new Result<Workspace>(new PermissionDeniedException()));

        Func<Task> act = async () => await _reelService.GetByWorkspaceAsync(workspaceId, userId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnReels()
    {
        var name = "Test";
        var reels = new List<Reel> { CreateTestReel() };
        _reelRepositoryMock.Setup(r => r.SearchByNameAsync(name, SearchOption.FullMatch, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reels);

        var result = await _reelService.SearchByNameAsync(name, SearchOption.FullMatch);

        result.Should().BeEquivalentTo(reels);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowValidationException_WhenReelIsInvalid()
    {
        var reel = CreateTestReel();
        reel.Name = string.Empty; // Invalid data

        Func<Task> act = async () => await _reelService.AddAsync(reel);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowValidationException_WhenReelIsInvalid()
    {
        var reel = CreateTestReel();
        _reelRepositoryMock.Setup(r => r.GetByIdAsync(reel.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reel);
        reel.Name = string.Empty; // Invalid data

        Func<Task> act = async () => await _reelService.UpdateAsync(reel);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task AddAsync_ShouldNotThrowValidationException_WhenReelIsValid()
    {
        var reel = CreateTestReel();

        Func<Task> act = async () => await _reelService.AddAsync(reel);

        await act.Should().NotThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotThrowValidationException_WhenReelIsValid()
    {
        var reel = CreateTestReel();
        _reelRepositoryMock.Setup(r => r.GetByIdAsync(reel.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reel);

        Func<Task> act = async () => await _reelService.UpdateAsync(reel);

        await act.Should().NotThrowAsync<ValidationException>();
    }

    private static Reel CreateTestReel(Guid? id = null, Guid? workspaceId = null, Guid? ownerId = null)
    {
        return new Reel
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Test Reel",
            WorkspaceId = workspaceId ?? Guid.NewGuid(),
            OwnerId = ownerId ?? Guid.NewGuid()
        };
    }
}