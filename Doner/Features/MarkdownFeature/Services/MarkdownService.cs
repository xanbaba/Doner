using Doner.Features.MarkdownFeature.Exceptions;
using Doner.Features.MarkdownFeature.Repositories;
using Doner.Features.MarkdownFeature.Validation;
using Doner.Features.WorkspaceFeature.Repository;
using FluentValidation;
using LanguageExt;
using LanguageExt.Common;

namespace Doner.Features.MarkdownFeature.Services;

public class MarkdownService : IMarkdownService
{
    private readonly IMarkdownRepository _markdownRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly MarkdownValidator _validator;

    public MarkdownService(
        IMarkdownRepository markdownRepository,
        IWorkspaceRepository workspaceRepository,
        MarkdownValidator validator)
    {
        _markdownRepository = markdownRepository;
        _workspaceRepository = workspaceRepository;
        _validator = validator;
    }

    public async Task<Result<IEnumerable<Markdown>>> GetMarkdownsByWorkspaceAsync(Guid workspaceId, Guid userId)
    {
        // Check if user is in workspace
        var hasAccess = await _workspaceRepository.IsUserInWorkspaceAsync(workspaceId, userId);
        if (!hasAccess)
        {
            return new Result<IEnumerable<Markdown>>(new PermissionDeniedException());
        }

        // Get markdowns
        var markdowns = await _markdownRepository.GetMarkdownsByOwnerAsync(userId);
        
        // Filter by workspace
        return new Result<IEnumerable<Markdown>>(markdowns.Where(m => m.WorkspaceId == workspaceId));
    }

    public async Task<Result<MarkdownMetadata>> GetMarkdownAsync(string markdownId, Guid workspaceId, Guid userId)
    {
        // Get markdown metadata
        var markdown = await _markdownRepository.GetMarkdownMetadataAsync(markdownId);
        if (markdown == null)
        {
            return new Result<MarkdownMetadata>(new MarkdownNotFoundException());
        }

        // Check if markdown belongs to the workspace
        if (markdown.WorkspaceId != workspaceId)
        {
            return new Result<MarkdownMetadata>(new MarkdownNotFoundException());
        }

        // Check if user has access to workspace
        var hasAccess = await _workspaceRepository.IsUserInWorkspaceAsync(workspaceId, userId);
        if (!hasAccess)
        {
            return new Result<MarkdownMetadata>(new PermissionDeniedException());
        }

        return markdown;
    }

    public async Task<Result<string>> CreateMarkdownAsync(string title, Guid workspaceId, Guid userId)
    {
        // Check if user has access to workspace
        var hasAccess = await _workspaceRepository.IsUserInWorkspaceAsync(workspaceId, userId);
        if (!hasAccess)
        {
            return new Result<string>(new PermissionDeniedException());
        }

        // Validate markdown
        var markdown = new Markdown
        {
            Title = title,
            OwnerId = userId,
            WorkspaceId = workspaceId
        };
        
        // Use ValidateAndThrow instead of manual validation
        await _validator.ValidateAndThrowAsync(markdown);

        // Create markdown
        var markdownId = await _markdownRepository.CreateMarkdownAsync(title, userId, workspaceId);
        
        // We don't have a way to get the created ID in this implementation
        // In a real application, the repository would return the ID
        return markdownId;
    }

    public async Task<Result<Unit>> UpdateMarkdownAsync(string markdownId, string title, Guid workspaceId, Guid userId)
    {
        // Get markdown metadata
        var markdown = await _markdownRepository.GetMarkdownMetadataAsync(markdownId);
        if (markdown == null)
        {
            return new Result<Unit>(new MarkdownNotFoundException());
        }

        // Check if markdown belongs to the workspace
        if (markdown.WorkspaceId != workspaceId)
        {
            return new Result<Unit>(new MarkdownNotFoundException());
        }

        // Check if user has access to workspace
        var hasAccess = await _workspaceRepository.IsUserInWorkspaceAsync(workspaceId, userId);
        if (!hasAccess)
        {
            return new Result<Unit>(new PermissionDeniedException());
        }

        // Validate markdown
        var updatedMarkdown = new Markdown
        {
            Id = markdownId,
            Title = title,
            OwnerId = markdown.OwnerId,
            WorkspaceId = workspaceId
        };
        
        // Use ValidateAndThrow instead of manual validation
        await _validator.ValidateAndThrowAsync(updatedMarkdown);

        // Update markdown
        await _markdownRepository.UpdateMarkdownAsync(markdownId, title);
        
        return Unit.Default;
    }

    public async Task<Result<Unit>> DeleteMarkdownAsync(string markdownId, Guid workspaceId, Guid userId)
    {
        // Get markdown metadata
        var markdown = await _markdownRepository.GetMarkdownMetadataAsync(markdownId);
        if (markdown == null)
        {
            return new Result<Unit>(new MarkdownNotFoundException());
        }

        // Check if markdown belongs to the workspace
        if (markdown.WorkspaceId != workspaceId)
        {
            return new Result<Unit>(new MarkdownNotFoundException());
        }

        // Check if user has access to workspace
        var hasAccess = await _workspaceRepository.IsUserInWorkspaceAsync(workspaceId, userId);
        if (!hasAccess)
        {
            return new Result<Unit>(new PermissionDeniedException());
        }

        // Delete markdown
        await _markdownRepository.DeleteMarkdownAsync(markdownId);
        
        return Unit.Default;
    }
}
