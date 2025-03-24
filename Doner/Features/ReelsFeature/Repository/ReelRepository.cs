using System.Text.RegularExpressions;
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
        return await _reelsMongoCollection.FindOneAndDeleteAsync(r => r.Id == id, cancellationToken: cancellationToken) != null;
    }

    public async Task<IEnumerable<Reel>> GetByWorkspaceAsync(Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var result = await _reelsMongoCollection.FindAsync(r => r.WorkspaceId == workspaceId, cancellationToken: cancellationToken);
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
}