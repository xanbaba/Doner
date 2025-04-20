namespace Doner.Features.MarkdownFeature;

public interface IOperationRepository
{
    Task<IEnumerable<Operation>> GetOperationsAsync(string markdownId, int afterVersion, CancellationToken cancellationToken = default);
    Task<Operation?> GetOperationAsync(Guid operationId, CancellationToken cancellationToken = default);
    Task AddOperationAsync(Operation operation, CancellationToken cancellationToken = default);
    Task<int> GetLatestVersionAsync(string markdownId, CancellationToken cancellationToken = default);
}