using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Doner.Features.MarkdownFeature;

public class Markdown
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid OwnerId { get; set; }
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid WorkspaceId { get; set; }
    
    public string Title { get; set; } = "";

    public List<char> Content { get; set; } = [];
    
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }
    
    public int Version { get; set; } = 0;
}