using MongoDB.Bson.Serialization.Attributes;

namespace Doner.Features.ReelsFeature.Elements;

[BsonDiscriminator(nameof(PlainText))]
public class PlainText
{
    public string Text { get; set; } = string.Empty;
}