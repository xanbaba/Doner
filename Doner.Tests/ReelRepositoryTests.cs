using Doner.Features.ReelsFeature;
using Doner.Features.ReelsFeature.Elements;
using Doner.Features.ReelsFeature.Repository;
using FluentAssertions;
using Mongo2Go;
using MongoDB.Driver;
using SearchOption = Contracts.V1.SearchOption;

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
            OwnerId = Guid.NewGuid()
        };
    }
    
    [Fact]
    public async Task GetReelElementAsync_ShouldReturnCorrectElement()
    {
        var reel = CreateTestReel();
        var element = CreateTestReelElement();
        reel.ReelElements = new List<ReelElement> { element };
        await _collection.InsertOneAsync(reel);

        var result = await _repository.GetReelElementAsync(reel.Id, element.Id);
        result.Should().NotBeNull();
        result.Data.Should().Be(element.Data);
        result.Id.Should().Be(element.Id);
    }

    [Fact]
    public async Task GetReelElementAsync_ShouldReturnNull_WhenElementNotFound()
    {
        var reel = CreateTestReel();
        await _collection.InsertOneAsync(reel);

        var result = await _repository.GetReelElementAsync(reel.Id, Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetReelElementsAsync_ShouldReturnAllElements()
    {
        var reel = CreateTestReel();
        var elements = new List<ReelElement> { CreateTestReelElement(), CreateTestReelElement() };
        reel.ReelElements = elements;
        await _collection.InsertOneAsync(reel);

        var result = (await _repository.GetReelElementsAsync(reel.Id)).ToList();

        result.Should().HaveCount(2);
        result.Select(e => e.Id).Should().BeEquivalentTo(elements.Select(e => e.Id));
        result.Select(e => e.Data).Should().BeEquivalentTo(elements.Select(e => e.Data));
    }

    [Fact]
    public async Task GetReelElementsAsync_ShouldReturnEmpty_WhenNoElements()
    {
        var reel = CreateTestReel();
        await _collection.InsertOneAsync(reel);

        var result = (await _repository.GetReelElementsAsync(reel.Id)).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AppendReelElementAsync_ShouldAddElement()
    {
        var reel = CreateTestReel();
        await _collection.InsertOneAsync(reel);

        var element = CreateTestReelElement();
        var result = await _repository.AppendReelElementAsync(reel.Id, element);

        result.Should().NotBeNull();
        result.Id.Should().Be(element.Id);

        var updatedReel = await _collection.Find(r => r.Id == reel.Id).FirstOrDefaultAsync();
        updatedReel!.ReelElements.Should().ContainSingle(e => e.Id == element.Id);
    }

    [Fact]
    public async Task UpdateReelElementAsync_ShouldUpdateElement()
    {
        var reel = CreateTestReel();
        var element = CreateTestReelElement();
        reel.ReelElements = new List<ReelElement> { element };
        await _collection.InsertOneAsync(reel);

        element.Data = "Updated Text";
        var result = await _repository.UpdateReelElementAsync(reel.Id, element);

        result.Should().NotBeNull();
        result.Data.Should().Be("Updated Text");

        var updatedReel = await _collection.Find(r => r.Id == reel.Id).FirstOrDefaultAsync();
        updatedReel!.ReelElements.First(e => e.Id == element.Id).Data.Should().Be("Updated Text");
    }

    [Fact]
    public async Task UpdateReelElementAsync_ShouldReturnNull_WhenElementNotFound()
    {
        var reel = CreateTestReel();
        await _collection.InsertOneAsync(reel);

        var element = CreateTestReelElement();
        var result = await _repository.UpdateReelElementAsync(reel.Id, element);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteReelElementAsync_ShouldRemoveElement()
    {
        var reel = CreateTestReel();
        var element = CreateTestReelElement();
        reel.ReelElements = new List<ReelElement> { element };
        await _collection.InsertOneAsync(reel);

        var success = await _repository.DeleteReelElementAsync(reel.Id, element.Id);
        success.Should().BeTrue();

        var updatedReel = await _collection.Find(r => r.Id == reel.Id).FirstOrDefaultAsync();
        updatedReel!.ReelElements.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteReelElementAsync_ShouldReturnFalse_WhenElementNotFound()
    {
        var reel = CreateTestReel();
        await _collection.InsertOneAsync(reel);

        var success = await _repository.DeleteReelElementAsync(reel.Id, Guid.NewGuid());
        success.Should().BeFalse();
    }
    
    [Fact]
    public async Task InsertReelElementAsync_ShouldInsertElement()
    {
        var reel = CreateTestReel();
        var existingElement = CreateTestReelElement();
        reel.ReelElements = new List<ReelElement> { existingElement };
        await _collection.InsertOneAsync(reel);
    
        var newElement = CreateTestReelElement();
        var result = await _repository.InsertReelElementAsync(reel.Id, existingElement.Id, newElement);
    
        result.Should().NotBeNull();
        result.Id.Should().Be(newElement.Id);
    
        var updatedReel = await _collection.Find(r => r.Id == reel.Id).FirstOrDefaultAsync();
        updatedReel!.ReelElements.Should().Contain(e => e.Id == newElement.Id);
        var existingElementIndex = updatedReel.ReelElements.FindIndex(e => e.Id == existingElement.Id);
        var newElementIndex = updatedReel.ReelElements.FindIndex(e => e.Id == newElement.Id);
        newElementIndex.Should().Be(existingElementIndex + 1);
    }
    
    [Fact]
    public async Task InsertReelElementAsync_ShouldReturnNull_WhenInsertAfterElementNotFound()
    {
        var reel = CreateTestReel();
        await _collection.InsertOneAsync(reel);
    
        var newElement = CreateTestReelElement();
        var result = await _repository.InsertReelElementAsync(reel.Id, Guid.NewGuid(), newElement);
    
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task PrependReelElementAsync_ShouldPrependElement()
    {
        var reel = CreateTestReel();
        await _collection.InsertOneAsync(reel);
    
        var element = CreateTestReelElement();
        var result = await _repository.PrependReelElementAsync(reel.Id, element);
    
        result.Should().NotBeNull();
        result.Id.Should().Be(element.Id);
    
        var updatedReel = await _collection.Find(r => r.Id == reel.Id).FirstOrDefaultAsync();
        updatedReel!.ReelElements.First().Id.Should().Be(element.Id);
    }
    
    [Fact]
    public async Task PrependReelElementAsync_ShouldReturnNull_WhenReelNotFound()
    {
        var element = CreateTestReelElement();
        var result = await _repository.PrependReelElementAsync(Guid.NewGuid(), element);
    
        result.Should().BeNull();
    }

    private ReelElement CreateTestReelElement()
    {
        return new ReelElement
        {
            Id = Guid.NewGuid(),
            Data = "Test Text"
        };
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}
