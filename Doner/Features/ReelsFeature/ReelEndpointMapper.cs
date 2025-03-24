using System.Security.Claims;
using Contracts.V1.Requests;
using Contracts.V1.Responses;
using Doner.Features.ReelsFeature.Services;
using Doner.Localizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Localization;

namespace Doner.Features.ReelsFeature;

public class ReelEndpointMapper : IEndpointMapper
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/me/workspaces/{workspaceId:guid}/reels", GetReels);
        builder.MapGet("/me/reels/{reelId:guid}", GetReelById).WithName(nameof(GetReelById));
        builder.MapPost("/me/workspaces/{workspaceId:guid}/reels", AddReel);
        builder.MapPut("/me/reels/{reelId:guid}", UpdateReel);
        builder.MapDelete("/me/reels/{reelId:guid}", DeleteReel);
    }

    private static async Task<Results<NotFound, NoContent, ForbidHttpResult>> DeleteReel
    (
        Guid reelId,
        IReelService reelService,
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
        Guid reelId,
        UpdateReelRequest request,
        IReelService reelService,
        ClaimsPrincipal user
    )
    {
        try
        {
            if (!await reelService.UpdateAsync(request.ToReel(reelId, user.GetUserId())))
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
        AddReelRequest request,
        IReelService reelService,
        ClaimsPrincipal user,
        Guid workspaceId,
        IStringLocalizer<Messages> localizer,
        CancellationToken cancellationToken
    )
    {
        var reel = request.ToReel(workspaceId, user.GetUserId());
        await reelService.AddAsync(reel, cancellationToken);
        return TypedResults.CreatedAtRoute(reel.ToResponse(), nameof(GetReelById), new { reelId = reel.Id });
    }

    private static async Task<Results<NotFound<string>, Ok<ReelResponse>, ForbidHttpResult>> GetReelById
    (
        Guid reelId,
        IReelService reelService,
        IStringLocalizer<Messages> localizer,
        ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var reel = await reelService.GetByIdAsync(reelId, user.GetUserId(), cancellationToken);
            if (reel is null)
            {
                return TypedResults.NotFound(localizer["ReelNotFound"].Value);
            }

            return TypedResults.Ok(reel.ToResponse());
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Forbid();
        }
    }

    private static async Task<Results<Ok<ReelsResponse>, ForbidHttpResult>> GetReels
    (
        GetReelsRequest request,
        IReelService reelService,
        ClaimsPrincipal user,
        Guid workspaceId,
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
    }
}