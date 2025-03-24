using Doner.Features.ReelsFeature.Elements;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Doner.Features.ReelsFeature;

public class Reel
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid WorkspaceId { get; set; }

    public Guid OwnerId { get; set; }
    public IEnumerable<ReelElement> ReelElements { get; set; } = [];
}