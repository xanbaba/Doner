using Doner.Features.WorkspaceFeature.Entities;

namespace Doner.Features.MarkdownFeature;

public class Markdown
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public string Uri { get; set; } = null!;
}