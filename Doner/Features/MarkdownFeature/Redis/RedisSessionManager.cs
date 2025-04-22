using StackExchange.Redis;
using Microsoft.Extensions.Options;

namespace Doner.Features.MarkdownFeature.Redis;

public class RedisSessionManager : ISessionManager
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisSessionManager> _logger;
    private readonly RedisOptions _options;
    
    // Keys and prefixes for Redis
    private const string SessionKeyPrefix = "markdown:session:";
    private const string SessionField = "active";
    
    public RedisSessionManager(
        IConnectionMultiplexer redis, 
        IOptions<RedisOptions> options,
        ILogger<RedisSessionManager> logger)
    {
        _redis = redis;
        _logger = logger;
        _options = options.Value;
    }
    
    private string GetSessionKey(string markdownId) => $"{SessionKeyPrefix}{markdownId}";
    
    public async Task OpenSessionAsync(string markdownId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        string sessionKey = GetSessionKey(markdownId);
        
        // Set session to active and set expiry
        await db.HashSetAsync(sessionKey, SessionField, "1");
        await db.KeyExpireAsync(sessionKey, _options.SessionTimeout);
        
        _logger.LogInformation("Opened session for markdown {MarkdownId}", markdownId);
    }
    
    public async Task CloseSessionAsync(string markdownId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        string sessionKey = GetSessionKey(markdownId);
        
        // Delete the session key
        await db.KeyDeleteAsync(sessionKey);
        
        _logger.LogInformation("Closed session for markdown {MarkdownId}", markdownId);
    }
    
    public async Task<bool> IsSessionActiveAsync(string markdownId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        string sessionKey = GetSessionKey(markdownId);
        
        // Check if the session key exists
        return await db.KeyExistsAsync(sessionKey);
    }
    
    public async Task RefreshSessionAsync(string markdownId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        string sessionKey = GetSessionKey(markdownId);
        
        // Extend the expiry time of the session
        bool success = await db.KeyExpireAsync(sessionKey, _options.SessionTimeout);
        
        if (success)
        {
            _logger.LogDebug("Refreshed session for markdown {MarkdownId}", markdownId);
        }
        else
        {
            _logger.LogWarning("Failed to refresh session for markdown {MarkdownId} - session may not exist", markdownId);
        }
    }
}
