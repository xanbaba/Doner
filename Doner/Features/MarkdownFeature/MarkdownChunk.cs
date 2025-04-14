namespace Doner.Features.MarkdownFeature;

public class MarkdownChunk
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = null!; // h1, h2, paragraph, list, code, etc.
    public string Content { get; set; } = null!;
    public int Order { get; set; }
}