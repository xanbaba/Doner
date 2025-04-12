using Contracts.V1.Requests;
using Contracts.V1.Responses;

namespace Doner.Features.MarkdownFeature;

public static class MarkdownMapper
{
    public static MarkdownResponse ToResponse(this Markdown markdown) =>
        new()
        {
            Id = markdown.Id,
            Name = markdown.Name,
            Uri = markdown.Uri
        };

    public static MarkdownsResponse ToResponse(this IEnumerable<Markdown> markdowns) =>
        new()
        {
            Items = markdowns.Select(ToResponse)
        };

    public static Markdown ToMarkdown(this AddMarkdownRequest request, Guid workspaceId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            WorkspaceId = workspaceId
        };

    public static Markdown ToMarkdown(this UpdateMarkdownRequest request, Guid id) =>
        new()
        {
            Id = id,
            Name = request.Name
        };
}