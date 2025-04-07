using System.Text.RegularExpressions;
using Doner.Features.ReelsFeature.Elements;
using MongoDB.Bson;
using MongoDB.Driver;
using SearchOption = Contracts.V1.SearchOption;

namespace Doner.Features.ReelsFeature.Repository;

public class ReelRepository : IReelRepository
{
    private readonly IMongoCollection<Reel> _reelsMongoCollection;

    public ReelRepository(IMongoCollection<Reel> reelsMongoCollection)
    {
        _reelsMongoCollection = reelsMongoCollection;
    }


    public async Task AddAsync(Reel reel, CancellationToken cancellationToken = default)
    {
        await _reelsMongoCollection.InsertOneAsync(reel, null, cancellationToken);
    }

    public async Task<Reel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var findCursor = await _reelsMongoCollection.FindAsync(r => r.Id == id, cancellationToken: cancellationToken);
        return await findCursor.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(Reel reel, CancellationToken cancellationToken = default)
    {
        var update = Builders<Reel>.Update
            .Set(r => r.Name, reel.Name)
            .Set(r => r.Description, reel.Description);
        return await _reelsMongoCollection.FindOneAndUpdateAsync(r => r.Id == reel.Id, update,
            cancellationToken: cancellationToken) != null;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _reelsMongoCollection.FindOneAndDeleteAsync(r => r.Id == id,
            cancellationToken: cancellationToken) != null;
    }

    public async Task<IEnumerable<Reel>> GetByWorkspaceAsync(Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var result =
            await _reelsMongoCollection.FindAsync(r => r.WorkspaceId == workspaceId,
                cancellationToken: cancellationToken);
        return result.ToList(cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<Reel>> SearchByNameAsync(string name, SearchOption searchOption,
        CancellationToken cancellationToken = default)
    {
        FilterDefinition<Reel> filter = searchOption switch
        {
            SearchOption.FullMatch => Builders<Reel>.Filter.Eq(r => r.Name, name),
            SearchOption.PartialMatch => Builders<Reel>.Filter.Regex(
                r => r.Name,
                new BsonRegularExpression($".*{Regex.Escape(name)}.*", "i")
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(searchOption))
        };

        return await _reelsMongoCollection
            .Find(filter)
            .ToListAsync(cancellationToken);
    }

    public async Task<ReelElement?> GetReelElementAsync(Guid reelId, Guid elementId,
        CancellationToken cancellationToken = default)
    {
        var reel = await _reelsMongoCollection.Find(r => r.Id == reelId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return reel?.ReelElements.FirstOrDefault(e => e.Id == elementId);
    }

    public async Task<IEnumerable<ReelElement>> GetReelElementsAsync(Guid reelId,
        CancellationToken cancellationToken = default)
    {
        var reel = await _reelsMongoCollection.Find(r => r.Id == reelId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return reel?.ReelElements ?? [];
    }

    public async Task<ReelElement?> AppendReelElementAsync(Guid reelId, ReelElement reelElement,
        CancellationToken cancellationToken = default)
    {
        var update = Builders<Reel>.Update.Push(r => r.ReelElements, reelElement);
        var result = await _reelsMongoCollection.FindOneAndUpdateAsync(r => r.Id == reelId, update,
            new FindOneAndUpdateOptions<Reel>
            {
                ReturnDocument = ReturnDocument.After
            }, cancellationToken: cancellationToken);
        return result?.ReelElements.FirstOrDefault(e => e.Id == reelElement.Id);
    }

    public async Task<ReelElement?> UpdateReelElementAsync(Guid reelId, ReelElement reelElement,
        CancellationToken cancellationToken = default)
    {
        var elementId = reelElement.Id;
        var filter = Builders<Reel>.Filter.And(
            Builders<Reel>.Filter.Eq(r => r.Id, reelId),
            Builders<Reel>.Filter.ElemMatch(r => r.ReelElements, e => e.Id == elementId)
        );

        var update = Builders<Reel>.Update.Set("ReelElements.$", reelElement);

        var options = new FindOneAndUpdateOptions<Reel>
        {
            ReturnDocument = ReturnDocument.After
        };

        var result = await _reelsMongoCollection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        return result?.ReelElements.FirstOrDefault(e => e.Id == elementId);
    }


    public async Task<bool> DeleteReelElementAsync(Guid reelId, Guid elementId,
        CancellationToken cancellationToken = default)
    {
        var update = Builders<Reel>.Update.PullFilter(r => r.ReelElements, e => e.Id == elementId);
        var result = await _reelsMongoCollection.UpdateOneAsync(
            r => r.Id == reelId && r.ReelElements.Any(e => e.Id == elementId),
            update, cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }

    public async Task<ReelElement?> InsertReelElementAsync(Guid reelId, Guid insertAfterElementId,
        ReelElement reelElement, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Reel>.Filter.And(
            Builders<Reel>.Filter.Eq(r => r.Id, reelId),
            Builders<Reel>.Filter.ElemMatch(r => r.ReelElements, e => e.Id == insertAfterElementId)
        );

        var update = Builders<Reel>.Update.PushEach(r => r.ReelElements, [reelElement], position: 1);

        var options = new FindOneAndUpdateOptions<Reel>
        {
            ReturnDocument = ReturnDocument.After
        };

        var result = await _reelsMongoCollection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        return result?.ReelElements.FirstOrDefault(e => e.Id == reelElement.Id);
    }

    public async Task<ReelElement?> PrependReelElementAsync(Guid reelId, ReelElement reelElement,
        CancellationToken cancellationToken = default)
    {
        var update = Builders<Reel>.Update.PushEach(r => r.ReelElements, [reelElement], position: 0);
        var result = await _reelsMongoCollection.FindOneAndUpdateAsync(r => r.Id == reelId, update,
            new FindOneAndUpdateOptions<Reel>
            {
                ReturnDocument = ReturnDocument.After
            }, cancellationToken: cancellationToken);
        return result?.ReelElements.FirstOrDefault(e => e.Id == reelElement.Id);
    }
}