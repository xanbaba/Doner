using MongoDB.Bson.Serialization.Attributes;

namespace Doner.Features.ReelsFeature.Elements;

[BsonDiscriminator(Required = true, RootClass = true)]
public abstract class ReelElement;