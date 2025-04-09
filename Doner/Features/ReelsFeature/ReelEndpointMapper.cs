using System.Security.Claims;
using Contracts.V1.Requests;
using Contracts.V1.Responses;
using Doner.Features.ReelsFeature.Services;
using Doner.Features.WorkspaceFeature.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Doner.Features.ReelsFeature;

public class ReelEndpointMapper : IEndpointMapper
{
    private const string ReelNotFound = "Reel not found.";
    
    public static void Map(IEndpointRouteBuilder builder)
    {
        var authGroup = builder.MapGroup("/users").RequireAuthorization();
        authGroup.MapGet("/me/workspaces/{workspaceId:guid}/reels", GetReels);
        authGroup.MapGet("/me/reels/{reelId:guid}", GetReelById).WithName(nameof(GetReelById));
        authGroup.MapPost("/me/workspaces/{workspaceId:guid}/reels", AddReel);
        authGroup.MapPut("/me/reels/{reelId:guid}", UpdateReel);
        authGroup.MapDelete("/me/reels/{reelId:guid}", DeleteReel);
    }

    private static async Task<Results<NotFound, NoContent, ForbidHttpResult>> DeleteReel
    (
        [FromRoute] Guid reelId,
        [FromServices] IReelService reelService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        try
        {
            if (!await reelService.DeleteAsync(reelId, user.GetUserId(), cancellationToken))
            {
                return TypedResults.NotFound();
            }

            return TypedResults.NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Forbid();
        }
    }

    private static async Task<Results<NotFound, NoContent, ForbidHttpResult>> UpdateReel
    (
        [FromRoute] Guid reelId,
        [FromBody] UpdateReelRequest request,
        [FromServices] IReelService reelService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        try
        {
            if (!await reelService.UpdateAsync(request.ToReel(reelId, user.GetUserId()), cancellationToken))
            {
                return TypedResults.NotFound();
            }
            return TypedResults.NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Forbid();
        }
    }

    private static async Task<CreatedAtRoute<ReelResponse>> AddReel
    (
        [FromBody] AddReelRequest request,
        [FromServices] IReelService reelService,
        ClaimsPrincipal user,
        [FromRoute] Guid workspaceId,
        CancellationToken cancellationToken
    )
    {
        var reel = request.ToReel(user.GetUserId(), workspaceId);
        await reelService.AddAsync(reel, cancellationToken);
        return TypedResults.CreatedAtRoute(reel.ToResponse(), nameof(GetReelById), new { reelId = reel.Id });
    }

    private static async Task<Results<NotFound<string>, Ok<ReelResponse>, ForbidHttpResult>> GetReelById
    (
        [FromRoute] Guid reelId,
        [FromServices] IReelService reelService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var reel = await reelService.GetByIdAsync(reelId, user.GetUserId(), cancellationToken);
            if (reel is null)
            {
                return TypedResults.NotFound(ReelNotFound);
            }

            return TypedResults.Ok(reel.ToResponse());
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Forbid();
        }
    }

    private static async Task<Results<Ok<ReelsResponse>, ForbidHttpResult, NotFound>> GetReels
    (
        [AsParameters] GetReelsRequest request,
        [FromServices] IReelService reelService,
        ClaimsPrincipal user,
        [FromRoute] Guid workspaceId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var reels = await reelService.GetByWorkspaceAsync(workspaceId, user.GetUserId(), cancellationToken);
            return TypedResults.Ok(reels.ToResponse());
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
}