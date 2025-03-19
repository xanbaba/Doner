using Doner.Features.ReelsFeature;
using Doner.Features.ReelsFeature.Services;
using FluentAssertions;
using Mongo2Go;
using MongoDB.Driver;
using SearchOption = Doner.Features.ReelsFeature.Services.SearchOption;

namespace Doner.Tests;

public class ReelRepositoryTests : IDisposable
{
    private readonly MongoDbRunner _runner;

    private readonly ReelRepository _repository;
    private readonly IMongoCollection<Reel> _collection;

    public ReelRepositoryTests()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        var database = client.GetDatabase("DonerTestDb");
        
        _collection = database.GetCollection<Reel>("Reels");
        _repository = new ReelRepository(_collection);
    }

    [Fact]
    public async Task AddAsync_ShouldInsertReel()
    {
        var reel = CreateTestReel();

        await _repository.AddAsync(reel);

        var inserted = await _collection.Find(r => r.Id == reel.Id).FirstOrDefaultAsync();
        inserted.Should().NotBeNull();
        inserted.Name.Should().Be(reel.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectReel()
    {
        var reel = CreateTestReel();
        await _collection.InsertOneAsync(reel);

        var result = await _repository.GetByIdAsync(reel.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(reel.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReplaceReel()
    {
        var reel = CreateTestReel();
        await _collection.InsertOneAsync(reel);

        reel.Name = "Updated Name";

        var success = await _repository.UpdateAsync(reel);
        success.Should().BeTrue();

        var updated = await _collection.Find(r => r.Id == reel.Id).FirstOrDefaultAsync();
        updated!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveReel()
    {
        var reel = CreateTestReel();
        await _collection.InsertOneAsync(reel);

        var success = await _repository.DeleteAsync(reel.Id);
        success.Should().BeTrue();

        var deleted = await _collection.Find(r => r.Id == reel.Id).FirstOrDefaultAsync();
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetByWorkspaceAsync_ShouldReturnAllReelsInWorkspace()
    {
        var workspaceId = Guid.NewGuid();
        var reels = Enumerable.Range(1, 3).Select(_ => CreateTestReel(workspaceId)).ToList();
        await _collection.InsertManyAsync(reels);

        var result = (await _repository.GetByWorkspaceAsync(workspaceId)).ToList();

        result.Should().HaveCount(3);
        result.All(r => r.WorkspaceId == workspaceId).Should().BeTrue();
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnExactMatches_WhenFullMatch()
    {
        var reel = CreateTestReel(name: "Cool Reel");
        await _collection.InsertOneAsync(reel);

        var result = (await _repository.SearchByNameAsync("Cool Reel", SearchOption.FullMatch)).ToList();

        result.Should().ContainSingle();
        result.First().Id.Should().Be(reel.Id);
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnPartialMatches_WhenPartialMatch()
    {
        var reel1 = CreateTestReel(name: "Cool Reel");
        var reel2 = CreateTestReel(name: "Super Cool Thing");
        var reel3 = CreateTestReel(name: "Unrelated");
        await _collection.InsertManyAsync([reel1, reel2, reel3]);

        var result = (await _repository.SearchByNameAsync("Cool", SearchOption.PartialMatch)).ToList();

        result.Should().HaveCount(2);
        result.Select(r => r.Id).Should().Contain([reel1.Id, reel2.Id]);
    }

    private Reel CreateTestReel(Guid? workspaceId = null, string? name = null)
    {
        return new Reel
        {
            Id = Guid.NewGuid(),
            Name = name ?? "Test Reel",
            WorkspaceId = workspaceId ?? Guid.NewGuid(),
            // Add other fields as needed
        };
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}
