using Doner.DataBase;
using Doner.Features.MarkdownFeature.Exceptions;
using Doner.Features.WorkspaceFeature.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Doner.Features.MarkdownFeature;

public class MarkdownService : IMarkdownService
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<Markdown> _markdownValidator;

    public MarkdownService(AppDbContext dbContext, IValidator<Markdown> markdownValidator)
    {
        _dbContext = dbContext;
        _markdownValidator = markdownValidator;
    }

    public async Task<Markdown> AddMarkdownAsync(Markdown markdown, Guid userId)
    {
        await ValidateUserAccessAsync(markdown.WorkspaceId, userId);
        await _markdownValidator.ValidateAndThrowAsync(markdown);

        _dbContext.Markdowns.Add(markdown);
        await _dbContext.SaveChangesAsync();
        return markdown;
    }

    public async Task<Markdown> GetMarkdownAsync(Guid id, Guid userId)
    {
        var markdown = await GetMarkdownAsync(id);
        await ValidateUserAccessAsync(markdown.WorkspaceId, userId);
        return markdown;
    }
    
    public async Task<Markdown> GetMarkdownAsync(Guid id)
    {
        var markdown = await _dbContext.Markdowns
            .Include(m => m.Workspace)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (markdown == null)
            throw new MarkdownNotFoundException();

        return markdown;
    }

    public async Task<Markdown> DeleteMarkdownAsync(Guid id, Guid userId)
    {
        var markdown = await GetMarkdownAsync(id);
        await ValidateUserAccessAsync(markdown.WorkspaceId, userId);

        _dbContext.Markdowns.Remove(markdown);
        await _dbContext.SaveChangesAsync();
        return markdown;
    }

    public async Task<Markdown> UpdateMarkdownAsync(Markdown markdown, Guid userId)
    {
        var existingMarkdown = await GetMarkdownAsync(markdown.Id);
        await ValidateUserAccessAsync(existingMarkdown.WorkspaceId, userId);
        await _markdownValidator.ValidateAndThrowAsync(markdown);

        existingMarkdown.Name = markdown.Name;
        existingMarkdown.Uri = markdown.Uri;

        _dbContext.Markdowns.Update(existingMarkdown);
        await _dbContext.SaveChangesAsync();
        return existingMarkdown;
    }

    public async Task<List<Markdown>> GetMarkdownsAsync(Guid workspaceId, Guid userId)
    {
        await ValidateUserAccessAsync(workspaceId, userId);

        return await _dbContext.Markdowns
            .Where(m => m.WorkspaceId == workspaceId)
            .ToListAsync();
    }

    private async Task ValidateUserAccessAsync(Guid workspaceId, Guid userId)
    {
        var workspace = await _dbContext.Workspaces
            .Include(w => w.Invitees)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            throw new WorkspaceNotFoundException();

        var isAuthorized = workspace.OwnerId == userId || workspace.Invitees.Any(i => i.UserId == userId);
        if (!isAuthorized)
            throw new UnauthorizedAccessException("User does not have access to this workspace.");
    }
}