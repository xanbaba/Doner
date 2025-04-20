using StackExchange.Redis;
using System.Text.Json;

namespace Doner.Features.MarkdownFeature.Redis;

public class RedisOperationRepository : IOperationRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisOperationRepository> _logger;
    private readonly ISessionManager _sessionManager;
    private readonly JsonSerializerOptions _jsonOptions;
    
    // Keys and prefixes for Redis
    private const string OperationsKeyPrefix = "markdown:operations:";
    private const string VersionKeyPrefix = "markdown:version:";
    private const string OperationKeyPrefix = "operation:";
    
    public RedisOperationRepository(
        IConnectionMultiplexer redis,
        ISessionManager sessionManager,
        ILogger<RedisOperationRepository> logger)
    {
        _redis = redis;
        _sessionManager = sessionManager;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
    
    private static string GetOperationsKey(string markdownId) => $"{OperationsKeyPrefix}{markdownId}";
    private static string GetVersionKey(string markdownId) => $"{VersionKeyPrefix}{markdownId}";
    private static string GetOperationKey(Guid operationId) => $"{OperationKeyPrefix}{operationId}";
    
    public async Task<IEnumerable<Operation>> GetOperationsAsync(
        string markdownId, 
        int afterVersion, 
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        string operationsKey = GetOperationsKey(markdownId);
        
        // Use sorted set to get operations with versions greater than afterVersion
        var operationEntries = await db.SortedSetRangeByScoreWithScoresAsync(
            operationsKey, 
            start: afterVersion + 1, // Exclusive lower bound
            stop: double.PositiveInfinity, // No upper bound
            exclude: Exclude.None, 
            order: Order.Ascending);
        
        if (operationEntries.Length == 0)
        {
            return [];
        }
        
        // Parse each operation from JSON
        var operations = new List<Operation>();
        foreach (var entry in operationEntries)
        {
            if (entry.Element.IsNull)
                continue;
            
            try
            {
                var operationJson = entry.Element.ToString();
                var operation = JsonSerializer.Deserialize<Operation>(operationJson, _jsonOptions);
                
                if (operation != null)
                {
                    operations.Add(operation);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize operation for markdown {MarkdownId}", markdownId);
            }
        }
        
        return operations;
    }
    
    public async Task<Operation?> GetOperationAsync(Guid operationId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        string operationKey = GetOperationKey(operationId);
        
        // Get the operation by its ID
        var operationJson = await db.StringGetAsync(operationKey);
        
        if (operationJson.IsNull)
        {
            return null;
        }
        
        try
        {
            return JsonSerializer.Deserialize<Operation>(operationJson!, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize operation with ID {OperationId}", operationId);
            return null;
        }
    }
    
    public async Task AddOperationAsync(Operation operation, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        string markdownId = operation.MarkdownId;
        
        // Check if session is active before adding operation
        if (!await _sessionManager.IsSessionActiveAsync(markdownId, cancellationToken))
        {
            // Automatically open a session if one doesn't exist
            await _sessionManager.OpenSessionAsync(markdownId, cancellationToken);
        }
        else
        {
            // Refresh the session to extend its TTL
            await _sessionManager.RefreshSessionAsync(markdownId, cancellationToken);
        }
        
        // Serialize the operation to JSON
        string operationJson = JsonSerializer.Serialize(operation, _jsonOptions);
        
        // Store in Redis transaction to ensure atomicity
        var transaction = db.CreateTransaction();
        
        // Add operation to the sorted set with score = baseVersion
        var operationsKey = GetOperationsKey(markdownId);
        await transaction.SortedSetAddAsync(
            operationsKey, 
            operationJson, 
            operation.BaseVersion);
        
        // Also store the operation by ID for direct lookups
        var operationKey = GetOperationKey(operation.Id);
        await transaction.StringSetAsync(
            operationKey, 
            operationJson);
        
        // Update the latest version if necessary
        var versionKey = GetVersionKey(markdownId);
        var latestVersion = await GetLatestVersionAsync(db, markdownId);
        if (latestVersion < operation.BaseVersion)
        {
            await transaction.StringSetAsync(
                versionKey,
                operation.BaseVersion.ToString());
        }

        // Execute transaction
        bool success = await transaction.ExecuteAsync();
        
        if (success)
        {
            _logger.LogInformation("Added operation {OperationId} for markdown {MarkdownId}", 
                operation.Id, markdownId);
        }
        else
        {
            _logger.LogWarning("Failed to add operation {OperationId} for markdown {MarkdownId}", 
                operation.Id, markdownId);
        }
    }
    
    public async Task<int> GetLatestVersionAsync(string markdownId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        return await GetLatestVersionAsync(db, markdownId);
    }

    private async Task<int> GetLatestVersionAsync(IDatabase db, string markdownId)
    {
        string versionKey = GetVersionKey(markdownId);
        
        // Get the latest version from Redis
        var versionValue = await db.StringGetAsync(versionKey);
        
        if (versionValue.IsNull)
        {
            // If no operations have been added yet, version is 0
            return 0;
        }
        
        if (int.TryParse(versionValue.ToString(), out int version))
        {
            return version;
        }
        
        _logger.LogWarning("Failed to parse version for markdown {MarkdownId}, returning 0", markdownId);
        return 0;
    }

}
