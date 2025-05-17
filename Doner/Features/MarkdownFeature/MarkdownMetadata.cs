using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Doner.Features.MarkdownFeature;

/// <summary>
/// Lightweight version of Markdown without content for metadata operations
/// </summary>
public class MarkdownMetadata
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = "";
    
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }
    public int Version { get; set; }
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid WorkspaceId { get; set; }
}
