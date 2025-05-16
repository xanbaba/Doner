namespace Contracts.V1.Responses;

public class MarkdownResponse
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required int Version { get; set; }
}
