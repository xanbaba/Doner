using Doner.Features.ReelsFeature.Elements;

namespace Doner.Features.ReelsFeature.Repository;

public interface IReelElementRepository
{
    Task<ReelElement?> GetReelElementAsync(Guid reelId, Guid elementId);
    Task<IEnumerable<ReelElement>> GetReelElementsAsync(Guid reelId);
    Task<ReelElement?> AddReelElementAsync(Guid reelId, ReelElement reelElement);
    Task<ReelElement?> UpdateReelElementAsync(Guid reelId, Guid elementId, ReelElement reelElement);
    Task<bool> DeleteReelElementAsync(Guid reelId, Guid elementId);
}