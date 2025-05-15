using Contracts.V1.Requests;
using Contracts.V1.Responses;
using Doner.Features.ReelsFeature.Elements;

namespace Doner.Features.ReelsFeature;

public static class ReelElementMapper
{
    public static ReelElementResponse ToResponse(this ReelElement reelElement) =>
        new()
        {
            Id = reelElement.Id,
            Data = reelElement.Data
        };

    public static ReelElementsResponse ToResponse(this IEnumerable<ReelElement> reelElements) =>
        new()
        {
            Items = reelElements.Select(ToResponse)
        };

    public static ReelElement ToReelElement(this AddReelElementRequest request) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            Data = request.Data
        };

    public static ReelElement ToReelElement(this UpdateReelElementRequest request, Guid id) =>
        new()
        {
            Id = id,
            Data = request.Data
        };
}