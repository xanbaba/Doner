namespace Doner.Features.ReelsFeature.Services;

public interface IReelRepository
{
    Task<bool> AddAsync(Reel reel);
    Task<Reel?> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(Reel reel);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Reel>> GetAsync();
    Task<IEnumerable<Reel>> GetByWorkspaceAsync(Guid workspaceId);
    Task<IEnumerable<Reel>> SearchByNameAsync(string name, SearchOption searchOption);
}