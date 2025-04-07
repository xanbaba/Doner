using Doner.Features.ReelsFeature.Elements;
using SearchOption = Contracts.V1.SearchOption;

namespace Doner.Features.ReelsFeature.Services;

public interface IReelService
{
    Task AddAsync(Reel reel, CancellationToken cancellationToken = default);
    Task<Reel?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Reel reel, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Reel>> GetByWorkspaceAsync(Guid workspaceId, Guid userId,
        CancellationToken cancellationToken = default);

    // TODO: Add corresponding endpoint and tests
    Task<IEnumerable<Reel>> SearchByNameAsync(string name, SearchOption searchOption,
        CancellationToken cancellationToken = default);
    
    Task<ReelElement?> GetReelElementAsync(Guid reelId, Guid elementId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReelElement>> GetReelElementsAsync(Guid reelId, Guid userId, CancellationToken cancellationToken = default);
    Task<ReelElement?> AppendReelElementAsync(Guid reelId, ReelElement reelElement, Guid userId, CancellationToken cancellationToken = default);
    Task<ReelElement?> PrependReelElementAsync(Guid reelId, ReelElement reelElement, Guid userId, CancellationToken cancellationToken = default);
    Task<ReelElement?> InsertReelElementAsync(Guid reelId, Guid insertAfterElementId, ReelElement reelElement, Guid userId, CancellationToken cancellationToken = default);
    Task<ReelElement?> UpdateReelElementAsync(Guid reelId, ReelElement reelElement, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteReelElementAsync(Guid reelId, Guid elementId, Guid userId, CancellationToken cancellationToken = default);
}