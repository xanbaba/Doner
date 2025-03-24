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
}