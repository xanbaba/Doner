namespace Doner.Features.MarkdownFeature;

public interface IMarkdownEntityService
{
    Task<Markdown> AddMarkdownAsync(Markdown markdown, Guid userId);
    Task<Markdown> GetMarkdownAsync(Guid id, Guid userId);
    Task<Markdown> DeleteMarkdownAsync(Guid id, Guid userId);
    Task<Markdown> UpdateMarkdownAsync(Markdown markdown, Guid userId);
    Task<List<Markdown>> GetMarkdownsAsync(Guid workspaceId, Guid userId);
}