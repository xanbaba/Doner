using JetBrains.Annotations;

namespace Doner.Features.MarkdownFeature.Hubs;

public record HubError([UsedImplicitly] int Code, [UsedImplicitly] string Message);