using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Doner.Features.MarkdownFeature;

public class Operation
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid MarkdownId { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserId { get; set; }

    public int BaseVersion { get; set; }
    public int ResultingVersion { get; set; }
    public DateTime Timestamp { get; set; }
        
    public List<OperationComponent> Components { get; set; } = [];
        
    // For operations that span multiple chunks
    public List<ChunkOperation> ChunkOperations { get; set; } = [];
}