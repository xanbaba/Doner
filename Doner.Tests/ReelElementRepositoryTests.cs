using Doner.Features.ReelsFeature;
using Doner.Features.ReelsFeature.Elements;
using Doner.Features.ReelsFeature.Repository;
using FluentAssertions;
using Mongo2Go;
using MongoDB.Driver;

namespace Doner.Tests;

public class ReelElementRepositoryTests : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly ReelRepository _repository;
    private readonly IMongoCollection<Reel> _collection;

    public ReelElementRepositoryTests()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        var database = client.GetDatabase("DonerTestDb");

        _collection = database.GetCollection<Reel>("Reels");
        _repository = new ReelRepository(_collection);
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
        result.Should().BeOfType<PlainText>();
        ((PlainText)result).Text.Should().Be(element.Text);
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
        result.Select(e => ((PlainText)e).Text).Should().BeEquivalentTo(elements.Select(e => ((PlainText)e).Text));
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

        element.Text = "Updated Text";
        var result = await _repository.UpdateReelElementAsync(reel.Id, element);

        result.Should().NotBeNull();
        result.Should().BeOfType<PlainText>();
        ((PlainText)result).Text.Should().Be("Updated Text");

        var updatedReel = await _collection.Find(r => r.Id == reel.Id).FirstOrDefaultAsync();
        ((PlainText)updatedReel!.ReelElements.First(e => e.Id == element.Id)).Text.Should().Be("Updated Text");
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

    private Reel CreateTestReel()
    {
        return new Reel
        {
            Id = Guid.NewGuid(),
            Name = "Test Reel",
            WorkspaceId = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            ReelElements = new List<ReelElement>()
        };
    }

    private PlainText CreateTestReelElement()
    {
        return new PlainText
        {
            Id = Guid.NewGuid(),
            Text = "Test Text"
        };
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}