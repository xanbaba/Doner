using MongoDB.Bson.Serialization.Attributes;

namespace Doner.Features.ReelsFeature.Elements;

[BsonDiscriminator(nameof(Picture))]
public class Picture
{
    public string Url { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}