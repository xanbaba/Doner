using Doner.Features.ReelsFeature;
using Doner.Features.ReelsFeature.Elements;
using Doner.Features.ReelsFeature.Repository;
using Doner.Features.ReelsFeature.Services;
using Doner.Features.ReelsFeature.Validation;
using Doner.Features.WorkspaceFeature.Entities;
using Doner.Features.WorkspaceFeature.Repository;
using Doner.Features.WorkspaceFeature.Services.WorkspaceService;
using FluentAssertions;
using FluentValidation;
using Mongo2Go;
using MongoDB.Driver;
using SearchOption = Contracts.V1.SearchOption;

namespace Doner.Tests;

public class ReelIntegrationTests : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly IMongoCollection<Reel> _reelCollection;
    private readonly ReelService _reelService;
    private readonly WorkspaceService _workspaceService;

    public ReelIntegrationTests()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        var database = client.GetDatabase("DonerTestDb");
        _reelCollection = database.GetCollection<Reel>("Reels");

        var reelRepository = new ReelRepository(_reelCollection);
        IValidator<Reel> reelValidator = new ReelValidator();
        IValidator<ReelElement> reelElementValidator = new CompositeReelElementValidator
        (
            new PictureValidator(),
            new CheckboxValidator(),
            new DropdownValidator(),
            new PlainTextValidator()
        );
        var appDbContextFactory = new AppDbContextFactory();
        var workspaceRepository = new WorkspaceRepository(appDbContextFactory);
        _workspaceService = new WorkspaceService(workspaceRepository);
        _reelService = new ReelService(reelRepository, reelValidator, reelElementValidator, _workspaceService);
    }

    [Fact]
    public async Task AddAsync_ShouldInsertReel_WhenValid()
    {
        var reel = CreateTestReel();

        await _reelService.AddAsync(reel);

        var inserted = await _reelCollection.Find(r => r.Id == reel.Id).FirstOrDefaultAsync();
        inserted.Should().NotBeNull();
        inserted.Name.Should().Be(reel.Name);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowValidationException_WhenInvalid()
    {
        var reel = CreateTestReel();
        reel.Name = string.Empty; // Invalid data

        Func<Task> act = async () => await _reelService.AddAsync(reel);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReel_WhenUserIsOwner()
    {
        var reel = CreateTestReel();
        await _reelCollection.InsertOneAsync(reel);

        var result = await _reelService.GetByIdAsync(reel.Id, reel.OwnerId);

        result.Should().NotBeNull();
        result.Id.Should().Be(reel.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        var reel = CreateTestReel();
        await _reelCollection.InsertOneAsync(reel);

        Func<Task> act = async () => await _reelService.GetByIdAsync(reel.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateReel_WhenUserIsOwner()
    {
        var id = Guid.NewGuid();
        var reel = CreateTestReel(id);
        await _reelCollection.InsertOneAsync(reel);
        reel.Name = "Updated Name";

        var result = await _reelService.UpdateAsync(reel);

        result.Should().BeTrue();
        var updated = await _reelCollection.Find(r => r.Id == reel.Id).FirstOrDefaultAsync();
        updated!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        var reel = CreateTestReel();
        await _reelCollection.InsertOneAsync(reel);
        reel.Name = "Updated Name";
        reel.OwnerId = Guid.NewGuid();

        Func<Task> act = async () => await _reelService.UpdateAsync(reel);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowValidationException_WhenInvalid()
    {
        var reel = CreateTestReel();
        await _reelCollection.InsertOneAsync(reel);
        reel.Name = string.Empty; // Invalid data

        Func<Task> act = async () => await _reelService.UpdateAsync(reel);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteReel_WhenUserIsOwner()
    {
        var reel = CreateTestReel();
        await _reelCollection.InsertOneAsync(reel);

        var result = await _reelService.DeleteAsync(reel.Id, reel.OwnerId);

        result.Should().BeTrue();
        var deleted = await _reelCollection.Find(r => r.Id == reel.Id).FirstOrDefaultAsync();
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        var reel = CreateTestReel();
        await _reelCollection.InsertOneAsync(reel);

        Func<Task> act = async () => await _reelService.DeleteAsync(reel.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetByWorkspaceAsync_ShouldReturnReels_WhenUserIsWorkspaceOwner()
    {
        var userId = Guid.NewGuid();
        var workspace = new Workspace
        {
            Name = "Test Workspace",
            OwnerId = userId
        };
        var createResult = await _workspaceService.CreateAsync(workspace);
        _ = createResult.IfFail(e => throw e);
        Guid? workspaceId = null;
        createResult.IfSucc(g => workspaceId = g);
        var reels = new List<Reel> { CreateTestReel(workspaceId: workspaceId, ownerId: userId) };
        await _reelCollection.InsertManyAsync(reels);

        var result = await _reelService.GetByWorkspaceAsync(workspaceId!.Value, userId);

        result.Should().BeEquivalentTo(reels);
    }

    [Fact]
    public async Task GetByWorkspaceAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotWorkspaceOwner()
    {
        var userId = Guid.NewGuid();
        var workspace = new Workspace
        {
            Name = "Test Workspace",
            OwnerId = userId
        };
        var createResult = await _workspaceService.CreateAsync(workspace);
        _ = createResult.IfFail(e => throw e);
        Guid? workspaceId = null;
        createResult.IfSucc(g => workspaceId = g);

        Func<Task> act = async () => await _reelService.GetByWorkspaceAsync(workspaceId!.Value, Guid.NewGuid());

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnReels()
    {
        var name = "Test";
        var reels = new List<Reel> { CreateTestReel(name: name) };
        await _reelCollection.InsertManyAsync(reels);

        var result = await _reelService.SearchByNameAsync(name, SearchOption.FullMatch);

        result.Should().BeEquivalentTo(reels);
    }

    private Reel CreateTestReel(Guid? id = null, Guid? workspaceId = null, Guid? ownerId = null, string? name = null)
    {
        return new Reel
        {
            Id = id ?? Guid.NewGuid(),
            Name = name ?? "Test Reel",
            Description = "Test Description",
            WorkspaceId = workspaceId ?? Guid.NewGuid(),
            OwnerId = ownerId ?? Guid.NewGuid()
        };
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}