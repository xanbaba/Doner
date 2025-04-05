using Doner.Features.ReelsFeature.Elements;
using SearchOption = Contracts.V1.SearchOption;

namespace Doner.Features.ReelsFeature.Repository;

public interface IReelRepository
{
    Task AddAsync(Reel reel, CancellationToken cancellationToken = default);
    Task<Reel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Reel reel, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reel>> GetByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reel>> SearchByNameAsync(string name, SearchOption searchOption, CancellationToken cancellationToken = default);

    Task<ReelElement?> AppendReelElementAsync(Guid reelId, ReelElement reelElement, CancellationToken cancellationToken = default);
    Task<ReelElement?> PrependReelElementAsync(Guid reelId, ReelElement reelElement, CancellationToken cancellationToken = default);
    Task<ReelElement?> InsertReelElementAsync(Guid reelId, Guid insertAfterElementId, ReelElement reelElement, CancellationToken cancellationToken = default);
    Task<ReelElement?> UpdateReelElementAsync(Guid reelId, ReelElement reelElement, CancellationToken cancellationToken = default);
    Task<bool> DeleteReelElementAsync(Guid reelId, Guid elementId, CancellationToken cancellationToken = default);
}