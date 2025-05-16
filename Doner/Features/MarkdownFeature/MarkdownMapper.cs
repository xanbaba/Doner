using Contracts.V1.Responses;

namespace Doner.Features.MarkdownFeature;

public static class MarkdownMapper
{
    public static MarkdownResponse ToResponse(this Markdown markdown)
    {
        return new MarkdownResponse
        {
            Id = markdown.Id,
            Title = markdown.Title,
            CreatedAt = markdown.CreatedAt,
            Version = markdown.Version
        };
    }
    
    public static MarkdownsResponse ToResponse(this IEnumerable<Markdown> markdowns)
    {
        return new MarkdownsResponse
        {
            Items = markdowns.Select(m => m.ToResponse())
        };
    }
    
    public static MarkdownResponse ToResponse(this MarkdownMetadata metadata)
    {
        return new MarkdownResponse
        {
            Id = metadata.Id,
            Title = metadata.Title,
            CreatedAt = metadata.CreatedAt,
            Version = metadata.Version
        };
    }
}
