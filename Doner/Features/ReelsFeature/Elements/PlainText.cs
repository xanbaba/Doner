using MongoDB.Bson.Serialization.Attributes;

namespace Doner.Features.ReelsFeature.Elements;

[BsonDiscriminator(nameof(PlainText))]
public class PlainText : ReelElement
{
    public required string Text { get; set; }
}