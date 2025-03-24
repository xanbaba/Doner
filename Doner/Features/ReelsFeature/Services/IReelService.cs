using SearchOption = Contracts.V1.SearchOption;

namespace Doner.Features.ReelsFeature.Services;

public interface IReelService
{
    Task AddAsync(Reel reel, CancellationToken cancellationToken = default);
    Task<Reel?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Reel reel, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Reel>> GetByWorkspaceAsync(Guid workspaceId, Guid userId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Reel>> SearchByNameAsync(string name, SearchOption searchOption,
        CancellationToken cancellationToken = default);
}