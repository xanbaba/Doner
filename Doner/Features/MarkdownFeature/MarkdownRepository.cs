using MongoDB.Bson;
using MongoDB.Driver;

namespace Doner.Features.MarkdownFeature;

public class MarkdownRepository : IMarkdownRepository
{
    private readonly IMongoCollection<Markdown> _markdownCollection;
    
    public MarkdownRepository(IMongoCollection<Markdown> markdownCollection)
    {
        _markdownCollection = markdownCollection;
        CreateIndexes();
    }
    
    private void CreateIndexes()
    {
        // Create indexes to optimize queries
        try
        {
            // Index for owner queries
            var ownerIdIndex = Builders<Markdown>.IndexKeys.Ascending(m => m.OwnerId);
            _markdownCollection.Indexes.CreateOneAsync(new CreateIndexModel<Markdown>(ownerIdIndex));
        }
        catch (MongoException)
        {
            // Indexes may already exist, ignore this error
        }
    }
    
    public async Task<string> GetMarkdownContentAsync(string markdownId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Markdown>.Filter.Eq(m => m.Id, markdownId);
        
        // Only project the content field to minimize data transfer
        var projection = Builders<Markdown>.Projection.Include(m => m.Content);
        
        var document = await _markdownCollection.Find(filter)
            .Project<ContentProjection>(projection)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (document == null)
        {
            return string.Empty;
        }

        return string.Join("", document.Content);
    }
    
    public async Task<IEnumerable<Markdown>> GetMarkdownsByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Markdown>.Filter.Eq(m => m.OwnerId, ownerId);
        
        // Exclude content field to minimize data transfer
        var projection = Builders<Markdown>.Projection
            .Exclude(m => m.Content);
        
        var markdowns = await _markdownCollection.Find(filter)
            .Project<Markdown>(projection)
            .ToListAsync(cancellationToken);
        
        return markdowns;
    }
    
    public async Task CreateMarkdownAsync(string title, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var markdown = new Markdown
        {
            OwnerId = ownerId,
            Title = title,
            CreatedAt = DateTime.UtcNow,
            Version = 0,
            Content = []
        };
        
        await _markdownCollection.InsertOneAsync(markdown, null, cancellationToken);
    }
    
    public async Task UpdateMarkdownAsync(string markdownId, string title, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Markdown>.Filter.Eq(m => m.Id, markdownId);
        var update = Builders<Markdown>.Update.Set(m => m.Title, title);
        
        await _markdownCollection.UpdateOneAsync(filter, update, null, cancellationToken);
    }
    
    public async Task DeleteMarkdownAsync(string markdownId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Markdown>.Filter.Eq(m => m.Id, markdownId);
        await _markdownCollection.DeleteOneAsync(filter, cancellationToken);
    }
    
    public async Task<MarkdownMetadata?> GetMarkdownMetadataAsync(string markdownId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Markdown>.Filter.Eq(m => m.Id, markdownId);
        
        // Project only metadata fields, excluding content
        var projection = Builders<Markdown>.Projection
            .Include(m => m.Id)
            .Include(m => m.OwnerId)
            .Include(m => m.Title)
            .Include(m => m.CreatedAt)
            .Include(m => m.Version)
            .Exclude(m => m.Content);
        
        var metadata = await _markdownCollection.Find(filter)
            .Project<MarkdownMetadata>(projection)
            .FirstOrDefaultAsync(cancellationToken);
        
        return metadata;
    }
    
    public async Task<bool> CheckVersionAsync(string markdownId, int expectedVersion, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Markdown>.Filter.Eq(m => m.Id, markdownId) & 
                     Builders<Markdown>.Filter.Eq(m => m.Version, expectedVersion);
        
        // Only count documents, don't fetch any data
        var count = await _markdownCollection.CountDocumentsAsync(filter, null, cancellationToken);
        return count > 0;
    }
    
    public async Task IncrementVersionAsync(string markdownId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Markdown>.Filter.Eq(m => m.Id, markdownId);
        
        // Use atomic increment operation
        var update = Builders<Markdown>.Update.Inc(m => m.Version, 1);
        
        await _markdownCollection.UpdateOneAsync(filter, update, null, cancellationToken);
    }
    
    public async Task<bool> InsertContentAsync(string markdownId, int position, string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(text))
        {
            return true; // Nothing to insert
        }
        
        var filter = Builders<Markdown>.Filter.Eq(m => m.Id, markdownId);
        
        // First, get the current content length without retrieving content
        var projection = Builders<Markdown>.Projection.Expression(
            doc => new ContentLengthProjection { ContentLength = doc.Content.Count });
        
        var result = await _markdownCollection.Find(filter)
            .Project(projection)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (result == null)
        {
            return false; // Document not found
        }
        
        int contentLength = result.ContentLength;
        
        // If the insertion point is at the end, use simple $push with $each
        if (position == contentLength)
        {
            // Now push the characters to insert
            var charsUpdate = Builders<Markdown>.Update.PushEach(m => m.Content, text.ToArray());
            await _markdownCollection.UpdateOneAsync(filter, charsUpdate, null, cancellationToken);
            
            return true;
        }
        
        // If the position is beyond content length, throw exception
        if (position > contentLength)
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Position is beyond content length");
        }
        
        // For insertion in the middle, use MongoDB's aggregation pipeline update
        
        // 1. Create update pipeline to modify the array in a single operation
        // This approach uses $concatArrays to join:
        // - The elements before the insertion point
        // - The new characters to insert
        // - The elements after the insertion point
        
        var updateDefinition = new BsonDocumentUpdateDefinition<Markdown>(
            new BsonDocument("$set", new BsonDocument
            {
                {
                    "Content", new BsonDocument("$concatArrays", new BsonArray
                    {
                        // First part: elements before insertion point
                        new BsonDocument("$slice", new BsonArray { "$Content", position }),
                        
                        // Middle part: new characters to insert
                        BsonArray.Create(text.ToArray()),
                        
                        // Last part: elements after insertion point
                        new BsonDocument("$slice", new BsonArray
                        {
                            "$Content", 
                            position, 
                            new BsonDocument("$subtract", new BsonArray
                            {
                                new BsonDocument("$size", "$Content"),
                                position
                            })
                        })
                    })
                }
            })
        );
        
        var updateResult = await _markdownCollection.UpdateOneAsync(
            filter, 
            updateDefinition, 
            null, 
            cancellationToken);
        
        return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
    }
    
    public async Task<bool> DeleteContentAsync(string markdownId, int position, int count, CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            return true; // Nothing to delete
        }
        
        var filter = Builders<Markdown>.Filter.Eq(m => m.Id, markdownId);
        
        // First, get the current content length
        var projection = Builders<Markdown>.Projection.Expression(
            doc => new ContentLengthProjection { ContentLength = doc.Content.Count });
        
        var result = await _markdownCollection.Find(filter)
            .Project(projection)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (result == null)
        {
            return false; // Document not found
        }
        
        int contentLength = result.ContentLength;
        
        // If the position is beyond content length, throw exception
        if (position >= contentLength)
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Position is beyond content length");
        }
        
        // Adjust count if it would exceed the content length
        count = Math.Min(count, contentLength - position);
        
        // Use MongoDB's aggregation pipeline update to perform the deletion in one operation
        
        var updateDefinition = new BsonDocumentUpdateDefinition<Markdown>(
            new BsonDocument("$set", new BsonDocument
            {
                {
                    "Content", new BsonDocument("$concatArrays", new BsonArray
                    {
                        // First part: elements before deletion point
                        new BsonDocument("$slice", new BsonArray { "$Content", position }),
                        
                        // Last part: elements after the deletion end point
                        new BsonDocument("$slice", new BsonArray
                        {
                            "$Content", 
                            position + count, 
                            new BsonDocument("$subtract", new BsonArray
                            {
                                new BsonDocument("$size", "$Content"),
                                position + count
                            })
                        })
                    })
                }
            })
        );
        
        var updateResult = await _markdownCollection.UpdateOneAsync(
            filter, 
            updateDefinition, 
            null, 
            cancellationToken);
        
        return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
    }
    
    // Helper classes for projections
    private class ContentProjection
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        public List<char> Content { get; set; } = [];
    }
    
    private class ContentLengthProjection
    {
        public int ContentLength { get; set; }
    }
}