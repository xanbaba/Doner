using MongoDB.Bson.Serialization.Attributes;

namespace Doner.Features.ReelsFeature.Elements;

[BsonDiscriminator(nameof(Dropdown))]
public class Dropdown
{
    public IEnumerable<ReelElement> Elements { get; set; } = [];
}