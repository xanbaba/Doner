using LanguageExt;
using LanguageExt.Common;

namespace Doner.Features.MarkdownFeature.Services;

public interface IMarkdownService
{
    Task<Result<IEnumerable<Markdown>>> GetMarkdownsByWorkspaceAsync(Guid workspaceId, Guid userId);
    Task<Result<MarkdownMetadata>> GetMarkdownAsync(string markdownId, Guid workspaceId, Guid userId);
    Task<Result<string>> CreateMarkdownAsync(string title, Guid workspaceId, Guid userId);
    Task<Result<Unit>> UpdateMarkdownAsync(string markdownId, string title, Guid workspaceId, Guid userId);
    Task<Result<Unit>> DeleteMarkdownAsync(string markdownId, Guid workspaceId, Guid userId);
}
