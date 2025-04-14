using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Doner.Features.MarkdownFeature;

public class MarkdownRepository : IMarkdownRepository
{
    private readonly IMongoCollection<Markdown> _markdowns;
    private readonly IMongoCollection<Operation> _operations;

    public MarkdownRepository(
        IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _markdowns = database.GetCollection<Markdown>("markdowns");
        _operations = database.GetCollection<Operation>("operations");
    }

    public async Task<List<Markdown>> GetAllAsync()
    {
        return await _markdowns.Find(_ => true).ToListAsync();
    }

    public async Task<Markdown?> GetByIdAsync(Guid id)
    {
        return await _markdowns.Find(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Markdown> CreateAsync(Markdown markdown)
    {
        markdown.CreatedAt = DateTime.UtcNow;
        await _markdowns.InsertOneAsync(markdown);
        return markdown;
    }

    public async Task UpdateAsync(Guid id, Markdown markdown)
    {
        await _markdowns.ReplaceOneAsync(m => m.Id == id, markdown);
    }

    public async Task<bool> UpdateChunksAsync(Guid id, List<MarkdownChunk> chunks)
    {
        var update = Builders<Markdown>.Update
            .Set(m => m.Chunks, chunks)
            .Inc(m => m.Version, 1);
            
        var result = await _markdowns.UpdateOneAsync(m => m.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _markdowns.DeleteOneAsync(m => m.Id == id);
        await _operations.DeleteManyAsync(o => o.MarkdownId == id);
    }

    public async Task<List<Operation>> GetOperationsAsync(Guid markdownId, int sinceVersion)
    {
        return await _operations
            .Find(o => o.MarkdownId == markdownId && o.ResultingVersion > sinceVersion)
            .SortBy(o => o.ResultingVersion)
            .ToListAsync();
    }

    public async Task SaveOperationAsync(Operation operation)
    {
        await _operations.InsertOneAsync(operation);
            
        // Update the document version
        var update = Builders<Markdown>.Update
            .Set(m => m.Version, operation.ResultingVersion);
            
        await _markdowns.UpdateOneAsync(m => m.Id == operation.MarkdownId, update);
    }
}