using System.Text.Json;
using Doner.DataBase;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Doner.Features.MarkdownFeature;

public class RedisConnectionTracker : IConnectionTracker
{
    private readonly IDatabase _db;
    private readonly AppDbContext _dbContext;
    
    // Redis key prefixes
    private const string ConnectionMappingKey = "markdown:connections";
    private const string DocumentConnectionsPrefix = "markdown:document:";
    private const string UserInfoMappingKey = "markdown:connections:userinfo";
    private const string ActiveDocumentsKey = "markdown:active_documents";
    
    public RedisConnectionTracker(IConnectionMultiplexer redis, AppDbContext dbContext)
    {
        _db = redis.GetDatabase();
        _dbContext = dbContext;
    }
    
    public async Task TrackConnectionAsync(string connectionId, string documentId, Guid userId)
    {
        // Fetch user data from database
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found.");
        }
        
        // Create user info from database user
        var userInfo = new UserInfo
        {
            UserId = user.Id,
            DisplayName = user.Username,
            Email = user.Email,
            ConnectedAt = DateTime.UtcNow
        };
        
        // Use transaction to ensure atomic operations
        var transaction = _db.CreateTransaction();
        
        // Map connection to document
        await transaction.HashSetAsync(ConnectionMappingKey, connectionId, documentId);
        
        // Add connection to document's set
        string documentConnectionsKey = $"{DocumentConnectionsPrefix}{documentId}:connections";
        await transaction.SetAddAsync(documentConnectionsKey, connectionId);
        
        // Store user info
        await transaction.HashSetAsync(UserInfoMappingKey, connectionId, JsonSerializer.Serialize(userInfo));
        
        // Add document to active documents set
        await transaction.SetAddAsync(ActiveDocumentsKey, documentId);
        
        // Execute all commands atomically
        await transaction.ExecuteAsync();
    }
    
    public async Task<string?> GetDocumentForConnectionAsync(string connectionId)
    {
        return await _db.HashGetAsync(ConnectionMappingKey, connectionId);
    }
    
    public async Task<IEnumerable<string>> GetConnectionsForDocumentAsync(string documentId)
    {
        string documentConnectionsKey = $"{DocumentConnectionsPrefix}{documentId}:connections";
        var connectionIds = await _db.SetMembersAsync(documentConnectionsKey);
        return connectionIds.Select(id => id.ToString());
    }
    
    public async Task RemoveConnectionAsync(string connectionId)
    {
        // Get document ID before removing the mapping
        string? documentId = await _db.HashGetAsync(ConnectionMappingKey, connectionId);
        
        if (string.IsNullOrEmpty(documentId))
            return;
            
        string documentConnectionsKey = $"{DocumentConnectionsPrefix}{documentId}:connections";
        
        // Use transaction for atomic operations
        var transaction = _db.CreateTransaction();
        
        // Remove from connection mapping
        await transaction.HashDeleteAsync(ConnectionMappingKey, connectionId);
        
        // Remove from document's connections set
        await transaction.SetRemoveAsync(documentConnectionsKey, connectionId);
        
        // Remove user info
        await transaction.HashDeleteAsync(UserInfoMappingKey, connectionId);
        
        // Execute transaction
        await transaction.ExecuteAsync();
        
        // Check if document has any remaining connections
        bool hasConnections = await _db.SetLengthAsync(documentConnectionsKey) > 0;
        
        if (!hasConnections)
        {
            // If no more connections, remove document from active documents
            await _db.SetRemoveAsync(ActiveDocumentsKey, documentId);
            
            // Optional: Clean up the empty set
            await _db.KeyDeleteAsync(documentConnectionsKey);
        }
    }
    
    public async Task<UserInfo?> GetUserInfoAsync(string connectionId)
    {
        string? userInfoJson = await _db.HashGetAsync(UserInfoMappingKey, connectionId);
        return string.IsNullOrEmpty(userInfoJson) 
            ? null 
            : JsonSerializer.Deserialize<UserInfo>(userInfoJson);
    }
    
    public async Task<IEnumerable<string>> GetActiveDocumentsAsync()
    {
        var documentIds = await _db.SetMembersAsync(ActiveDocumentsKey);
        return documentIds.Select(id => id.ToString());
    }
}