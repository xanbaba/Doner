using MongoDB.Driver;
using Doner.Features.ReelsFeature.Elements;

namespace Doner.Features.ReelsFeature.Repository;

public class ReelElementRepository : IReelElementRepository
{
    private readonly IMongoCollection<Reel> _reelsMongoCollection;

    public ReelElementRepository(IMongoCollection<Reel> reelsMongoCollection)
    {
        _reelsMongoCollection = reelsMongoCollection;
    }

    public async Task<ReelElement?> GetReelElementAsync(Guid reelId, Guid elementId)
    {
        var reel = await _reelsMongoCollection.Find(r => r.Id == reelId).FirstOrDefaultAsync();
        return reel?.ReelElements.FirstOrDefault(e => e.Id == elementId);
    }

    public async Task<IEnumerable<ReelElement>> GetReelElementsAsync(Guid reelId)
    {
        var reel = await _reelsMongoCollection.Find(r => r.Id == reelId).FirstOrDefaultAsync();
        return reel?.ReelElements ?? [];
    }

    public async Task<ReelElement?> AppendReelElementAsync(Guid reelId, ReelElement reelElement)
    {
        var update = Builders<Reel>.Update.Push(r => r.ReelElements, reelElement);
        var result = await _reelsMongoCollection.FindOneAndUpdateAsync(r => r.Id == reelId, update, new FindOneAndUpdateOptions<Reel>
        {
            ReturnDocument = ReturnDocument.After
        });
        return result?.ReelElements.FirstOrDefault(e => e.Id == reelElement.Id);
    }

    public async Task<ReelElement?> UpdateReelElementAsync(Guid reelId, Guid elementId, ReelElement reelElement)
    {
        var filter = Builders<Reel>.Filter.And(
            Builders<Reel>.Filter.Eq(r => r.Id, reelId),
            Builders<Reel>.Filter.ElemMatch(r => r.ReelElements, e => e.Id == elementId)
        );

        var update = Builders<Reel>.Update.Set("ReelElements.$", reelElement);

        var options = new FindOneAndUpdateOptions<Reel>
        {
            ReturnDocument = ReturnDocument.After
        };

        var result = await _reelsMongoCollection.FindOneAndUpdateAsync(filter, update, options);
        return result?.ReelElements.FirstOrDefault(e => e.Id == elementId);
    }


    public async Task<bool> DeleteReelElementAsync(Guid reelId, Guid elementId)
    {
        var update = Builders<Reel>.Update.PullFilter(r => r.ReelElements, e => e.Id == elementId);
        var result = await _reelsMongoCollection.UpdateOneAsync(
            r => r.Id == reelId && r.ReelElements.Any(e => e.Id == elementId),
            update
        );

        return result.ModifiedCount > 0;
    }
    
    public async Task<ReelElement?> InsertReelElementAsync(Guid reelId, Guid insertAfterElementId, ReelElement reelElement)
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

        var result = await _reelsMongoCollection.FindOneAndUpdateAsync(filter, update, options);
        return result?.ReelElements.FirstOrDefault(e => e.Id == reelElement.Id);
    }

    public async Task<ReelElement?> PrependReelElementAsync(Guid reelId, ReelElement reelElement)
    {
        var update = Builders<Reel>.Update.PushEach(r => r.ReelElements, [reelElement], position: 0);
        var result = await _reelsMongoCollection.FindOneAndUpdateAsync(r => r.Id == reelId, update, new FindOneAndUpdateOptions<Reel>
        {
            ReturnDocument = ReturnDocument.After
        });
        return result?.ReelElements.FirstOrDefault(e => e.Id == reelElement.Id);
    }
}