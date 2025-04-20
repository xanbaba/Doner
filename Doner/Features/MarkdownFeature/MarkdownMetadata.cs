namespace Doner.Features.MarkdownFeature;

/// <summary>
/// Lightweight version of Markdown without content for metadata operations
/// </summary>
public class MarkdownMetadata
{
    public string Id { get; set; } = null!;
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public int Version { get; set; }
}
