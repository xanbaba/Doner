using MongoDB.Bson.Serialization.Attributes;

namespace Doner.Features.ReelsFeature.Elements;

[BsonDiscriminator(nameof(Picture))]
public class Picture : ReelElement
{
    public required string Url { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
    public string? Caption { get; set; }
}