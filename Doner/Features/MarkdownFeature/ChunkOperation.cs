namespace Doner.Features.MarkdownFeature;

public class ChunkOperation
{
    public string ChunkId { get; set; } = null!;
    public int StartPosition { get; set; }
    public List<OperationComponent> Components { get; set; } = [];
}