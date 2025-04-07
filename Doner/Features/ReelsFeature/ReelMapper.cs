using Contracts.V1.Requests;
using Contracts.V1.Responses;

namespace Doner.Features.ReelsFeature;

public static class ReelMapper
{
    public static ReelResponse ToResponse(this Reel reel) =>
        new()
        {
            Id = reel.Id,
            Name = reel.Name,
            Description = reel.Description,
            WorkspaceId = reel.WorkspaceId,
            OwnerId = reel.OwnerId
        };

    public static ReelsResponse ToResponse(this IEnumerable<Reel> reels) =>
        new()
        {
            Items = reels.Select(ToResponse)
        };

    public static Reel ToReel(this AddReelRequest request, Guid ownerId, Guid workspaceId) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name,
            Description = request.Description,
            WorkspaceId = workspaceId,
            OwnerId = ownerId
        };

    public static Reel ToReel(this UpdateReelRequest request, Guid id, Guid ownerId)
    {
        var reel = new Reel
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            OwnerId = ownerId,
        };

        return reel;
    }
}