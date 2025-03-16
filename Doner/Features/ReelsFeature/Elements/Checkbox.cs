using MongoDB.Bson.Serialization.Attributes;

namespace Doner.Features.ReelsFeature.Elements;

[BsonDiscriminator(nameof(Checkbox))]
public class Checkbox : ReelElement
{
    public bool IsChecked { get; set; }
    public string Header { get; set; } = string.Empty;
}