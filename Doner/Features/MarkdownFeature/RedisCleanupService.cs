using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Doner.Features.MarkdownFeature;

/// <summary>
/// Background service that cleans up operations for expired sessions
/// </summary>
public class RedisCleanupService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCleanupService> _logger;
    private readonly RedisOptions _options;
    
    // Cleanup interval
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);
    
    public RedisCleanupService(
        IConnectionMultiplexer redis,
        IOptions<RedisOptions> options,
        ILogger<RedisCleanupService> logger)
    {
        _redis = redis;
        _logger = logger;
        _options = options.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Redis cleanup service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Perform cleanup
                await CleanupExpiredOperationsAsync();
                
                // Wait for the next cleanup interval
                await Task.Delay(CleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Redis cleanup service");
                
                // Wait a shorter time after an error
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        
        _logger.LogInformation("Redis cleanup service stopped");
    }
    
    private async Task CleanupExpiredOperationsAsync()
    {
        // This method would identify and clean up expired operations
        // For a more advanced implementation, consider using Redis keyspace notifications
        
        var db = _redis.GetDatabase();
        var server = _redis.GetServer(_redis.GetEndPoints()[0]);
        
        // Find all operation keys that might be candidates for cleanup
        var operationKeysPattern = "markdown:operations:*";
        var operationKeys = server.Keys(pattern: operationKeysPattern).ToArray();
        
        int cleanedCount = 0;
        
        foreach (var key in operationKeys)
        {
            // Extract the markdown ID from the key
            string markdownId = key.ToString().Replace("markdown:operations:", "");
            
            // Check if the session for this markdown is active
            string sessionKey = $"markdown:session:{markdownId}";
            bool sessionExists = await db.KeyExistsAsync(sessionKey);
            
            if (!sessionExists)
            {
                // Session has expired, set an expiry on the operations key
                var retention = _options.OperationRetention;
                await db.KeyExpireAsync(key, retention);
                cleanedCount++;
            }
        }
        
        if (cleanedCount > 0)
        {
            _logger.LogInformation("Set expiry on {Count} expired operation sets", cleanedCount);
        }
    }
}
