using Doner.Features.ReelsFeature.Elements;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Doner.Features.ReelsFeature;

public class Reel
{
    [BsonId]
    [BsonRepresentation(BsonType.Binary)]
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    [BsonRepresentation(BsonType.Binary)]
    public Guid WorkspaceId { get; set; }
    public IEnumerable<ReelElement> ReelElements { get; set; } = [];
}