using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Doner.Features.MarkdownFeature;

public class Markdown
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid CreatedById { get; set; }

    public List<MarkdownChunk> Chunks { get; set; } = [];

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public List<Guid> CollaboratorIds { get; set; } = [];

    public int Version { get; set; }
}