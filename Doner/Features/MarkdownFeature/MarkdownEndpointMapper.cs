using System.Security.Claims;
using Contracts.V1.Requests;
using Contracts.V1.Responses;
using Doner.Features.MarkdownFeature.Exceptions;
using Doner.Features.WorkspaceFeature.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Doner.Features.MarkdownFeature;

public class MarkdownEndpointMapper : IEndpointMapper
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        var authGroup = builder.MapGroup("/users").RequireAuthorization();
        authGroup.MapGet("/me/workspaces/{workspaceId:guid}/markdowns", GetMarkdowns);
        authGroup.MapGet("/me/markdowns/{markdownId:guid}", GetMarkdownById).WithName(nameof(GetMarkdownById));
        authGroup.MapPost("/me/workspaces/{workspaceId:guid}/markdowns", AddMarkdown);
        authGroup.MapPut("/me/markdowns/{markdownId:guid}", UpdateMarkdown);
        authGroup.MapDelete("/me/markdowns/{markdownId:guid}", DeleteMarkdown);
    }

    private static async Task<Results<Ok<MarkdownsResponse>, ForbidHttpResult, NotFound>> GetMarkdowns
    (
        [FromRoute] Guid workspaceId,
        [FromServices] IMarkdownEntityService markdownEntityService,
        ClaimsPrincipal user
    )
    {
        try
        {
            var markdowns = await markdownEntityService.GetMarkdownsAsync(workspaceId, user.GetUserId());
            return TypedResults.Ok(markdowns.ToResponse());
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Forbid();
        }
        catch (WorkspaceNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    private static async Task<Results<Ok<MarkdownResponse>, ForbidHttpResult, NotFound>> GetMarkdownById
    (
        [FromRoute] Guid markdownId,
        [FromServices] IMarkdownEntityService markdownEntityService,
        ClaimsPrincipal user
    )
    {
        try
        {
            var markdown = await markdownEntityService.GetMarkdownAsync(markdownId, user.GetUserId());
            return TypedResults.Ok(markdown.ToResponse());
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Forbid();
        }
        catch (MarkdownNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    private static async Task<Results<CreatedAtRoute<MarkdownResponse>, ForbidHttpResult, NotFound>> AddMarkdown
    (
        [FromRoute] Guid workspaceId,
        [FromBody] AddMarkdownRequest request,
        [FromServices] IMarkdownEntityService markdownEntityService,
        ClaimsPrincipal user
    )
    {
        try
        {
            var markdown = request.ToMarkdown(workspaceId);
            var createdMarkdown = await markdownEntityService.AddMarkdownAsync(markdown, user.GetUserId());
            return TypedResults.CreatedAtRoute(createdMarkdown.ToResponse(), nameof(GetMarkdownById), new { markdownId = createdMarkdown.Id });
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Forbid();
        }
        catch (WorkspaceNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    private static async Task<Results<NoContent, ForbidHttpResult, NotFound>> UpdateMarkdown
    (
        [FromRoute] Guid markdownId,
        [FromBody] UpdateMarkdownRequest request,
        [FromServices] IMarkdownEntityService markdownEntityService,
        ClaimsPrincipal user
    )
    {
        try
        {
            var markdown = request.ToMarkdown(markdownId);
            await markdownEntityService.UpdateMarkdownAsync(markdown, user.GetUserId());
            return TypedResults.NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Forbid();
        }
        catch (MarkdownNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    private static async Task<Results<NoContent, ForbidHttpResult, NotFound>> DeleteMarkdown
    (
        [FromRoute] Guid markdownId,
        [FromServices] IMarkdownEntityService markdownEntityService,
        ClaimsPrincipal user
    )
    {
        try
        {
            await markdownEntityService.DeleteMarkdownAsync(markdownId, user.GetUserId());
            return TypedResults.NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Forbid();
        }
        catch (MarkdownNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}