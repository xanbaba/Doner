namespace Contracts.V1.Responses;

public class MarkdownResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Uri { get; set; } = null!;
}