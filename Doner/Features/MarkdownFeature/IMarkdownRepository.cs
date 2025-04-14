namespace Doner.Features.MarkdownFeature;

public interface IMarkdownRepository
{
    Task<Markdown?> GetByIdAsync(Guid id);
    Task<List<Markdown>> GetAllAsync();
    Task<Markdown> CreateAsync(Markdown markdown);
    Task UpdateAsync(Guid id, Markdown markdown);
    Task<bool> UpdateChunksAsync(Guid id, List<MarkdownChunk> chunks);
    Task DeleteAsync(Guid id);
    Task<List<Operation>> GetOperationsAsync(Guid markdownId, int sinceVersion);
    Task SaveOperationAsync(Operation operation);
}