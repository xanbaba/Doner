using System.Security.Claims;
using Contracts.V1.Requests;
using Contracts.V1.Responses;
using Doner.Features.MarkdownFeature.Exceptions;
using Doner.Features.MarkdownFeature.Services;
using Doner.Resources;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Doner.Features.MarkdownFeature;

public class MarkdownEndpointMapper : IEndpointMapper
{
    private const string MarkdownNotFound = "Markdown does not exist.";

    public static void Map(IEndpointRouteBuilder builder)
    {
        var markdownGroup = builder.MapGroup("/users/me/workspaces/{workspaceId:guid}/markdowns").RequireAuthorization();

        markdownGroup.MapGet("/", GetMarkdownsAsync);
        markdownGroup.MapGet("/{markdownId}", GetMarkdownAsync).WithName(nameof(GetMarkdownAsync));
        markdownGroup.MapPost("/", CreateMarkdownAsync);
        markdownGroup.MapPut("/{markdownId}", UpdateMarkdownAsync);
        markdownGroup.MapDelete("/{markdownId}", DeleteMarkdownAsync);
    }

    private static async Task<Results<Ok<MarkdownsResponse>, BadRequest<string>>> GetMarkdownsAsync(
        [FromServices] IMarkdownService markdownService,
        [FromRoute] Guid workspaceId,
        ClaimsPrincipal user)
    {
        var markdownsResult = await markdownService.GetMarkdownsByWorkspaceAsync(workspaceId, user.GetUserId());

        return markdownsResult.Match<Results<Ok<MarkdownsResponse>, BadRequest<string>>>(
            markdowns => TypedResults.Ok(markdowns.ToResponse()),
            exception => exception switch
            {
                PermissionDeniedException => TypedResults.BadRequest(SharedResources.PermissionDenied),
                _ => throw exception
            });
    }

    private static async Task<Results<NotFound<string>, Ok<MarkdownResponse>, BadRequest<string>>> GetMarkdownAsync(
        [FromServices] IMarkdownService markdownService,
        [FromRoute] Guid workspaceId,
        [FromRoute] string markdownId,
        ClaimsPrincipal user)
    {
        var markdownResult = await markdownService.GetMarkdownAsync(markdownId, workspaceId, user.GetUserId());

        return markdownResult.Match<Results<NotFound<string>, Ok<MarkdownResponse>, BadRequest<string>>>(
            markdown => TypedResults.Ok(markdown.ToResponse()),
            exception => exception switch
            {
                MarkdownNotFoundException => TypedResults.NotFound(MarkdownNotFound),
                PermissionDeniedException => TypedResults.BadRequest(SharedResources.PermissionDenied),
                _ => throw exception
            });
    }

    private static async Task<Results<BadRequest<string>, NotFound<string>, CreatedAtRoute<MarkdownResponse>>> CreateMarkdownAsync(
        [FromServices] IMarkdownService markdownService,
        [FromRoute] Guid workspaceId,
        [FromBody] AddMarkdownRequest request,
        ClaimsPrincipal user)
    {
        var markdownResult = await markdownService.CreateMarkdownAsync(request.Title, workspaceId, user.GetUserId());

        return markdownResult.Match<Results<BadRequest<string>, NotFound<string>, CreatedAtRoute<MarkdownResponse>>>(
            markdownId =>
            {
                // In a real implementation, we would fetch the created markdown and return it
                // For now, we create a simple response
                var response = new MarkdownResponse
                {
                    Id = markdownId,
                    Title = request.Title,
                    CreatedAt = DateTime.UtcNow,
                    Version = 0
                };
                
                return TypedResults.CreatedAtRoute(response, nameof(GetMarkdownAsync), 
                    new { workspaceId, markdownId });
            },
            exception => exception switch
            {
                PermissionDeniedException => TypedResults.BadRequest(SharedResources.PermissionDenied),
                _ => throw exception
            });
    }

    private static async Task<Results<NoContent, BadRequest<string>, NotFound<string>>> UpdateMarkdownAsync(
        [FromServices] IMarkdownService markdownService,
        [FromRoute] Guid workspaceId,
        [FromRoute] string markdownId,
        [FromBody] UpdateMarkdownRequest request,
        ClaimsPrincipal user)
    {
        var markdownResult = await markdownService.UpdateMarkdownAsync(markdownId, request.Title, workspaceId, user.GetUserId());

        return markdownResult.Match<Results<NoContent, BadRequest<string>, NotFound<string>>>(
            _ => TypedResults.NoContent(),
            exception => exception switch
            {
                MarkdownNotFoundException => TypedResults.NotFound(MarkdownNotFound),
                PermissionDeniedException => TypedResults.BadRequest(SharedResources.PermissionDenied),
                _ => throw exception
            });
    }

    private static async Task<Results<NotFound<string>, NoContent, BadRequest<string>>> DeleteMarkdownAsync(
        [FromServices] IMarkdownService markdownService,
        [FromRoute] Guid workspaceId,
        [FromRoute] string markdownId,
        ClaimsPrincipal user)
    {
        var markdownResult = await markdownService.DeleteMarkdownAsync(markdownId, workspaceId, user.GetUserId());

        return markdownResult.Match<Results<NotFound<string>, NoContent, BadRequest<string>>>(
            _ => TypedResults.NoContent(),
            exception => exception switch
            {
                MarkdownNotFoundException => TypedResults.NotFound(MarkdownNotFound),
                PermissionDeniedException => TypedResults.BadRequest(SharedResources.PermissionDenied),
                _ => throw exception
            });
    }
}