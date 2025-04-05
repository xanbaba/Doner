using MongoDB.Bson.Serialization.Attributes;

namespace Doner.Features.ReelsFeature.Elements;

[BsonDiscriminator(nameof(Checkbox))]
public class Checkbox : ReelElement
{
    public bool IsChecked { get; set; }
    public required string Header { get; set; }
}