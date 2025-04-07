using System.Security.Claims;
using Contracts.V1.Requests;
using Contracts.V1.Responses;
using Doner.Features.ReelsFeature.Elements;
using Doner.Features.ReelsFeature.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Doner.Features.ReelsFeature;

public class ReelElementEndpointMapper : IEndpointMapper
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        var authGroup = builder.MapGroup("/users").RequireAuthorization();
        authGroup.MapGet("/me/reels/{reelId:guid}/elements", GetReelElements);
        authGroup.MapGet("/me/reels/{reelId:guid}/elements/{elementId:guid}", GetReelElement)
            .WithName(nameof(GetReelElement));
        authGroup.MapPost("/me/reels/{reelId:guid}/elements", AddReelElement);
        authGroup.MapPut("/me/reels/{reelId:guid}/elements/{elementId:guid}", UpdateReelElement);
        authGroup.MapDelete("/me/reels/{reelId:guid}/elements/{elementId:guid}", DeleteReelElement);
    }

    private static async Task<Results<Ok<ReelElementsResponse>, ForbidHttpResult>> GetReelElements
    (
        [FromRoute] Guid reelId,
        [FromServices] IReelService reelService,
        [FromServices] ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        try
        {
            IEnumerable<ReelElement> result =
                await reelService.GetReelElementsAsync(reelId, user.GetUserId(), cancellationToken);
            return TypedResults.Ok(result.ToResponse());
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Forbid();
        }
    }

    private static async Task<Results<Ok<ReelElementResponse>, ForbidHttpResult, NotFound>> GetReelElement
    (
        [FromRoute] Guid reelId,
        [FromRoute] Guid elementId,
        [FromServices] IReelService reelService,
        [FromServices] ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        try
        {
            ReelElement? result =
                await reelService.GetReelElementAsync(reelId, elementId, user.GetUserId(), cancellationToken);
            if (result is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(result.ToResponse());
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Forbid();
        }
    }

    private static async Task<Results<CreatedAtRoute<ReelElementResponse>, ForbidHttpResult, NotFound>> AddReelElement
    (
        [FromRoute] Guid reelId,
        [FromBody] AddReelElementRequest request,
        [FromServices] IReelService reelService,
        [FromServices] ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        try
        {
            if (request.AddType is AddType.Insert && request.InsertAfterId is null)
            {
                throw new ValidationException([
                    new ValidationFailure(nameof(request.InsertAfterId),
                        "InsertAfterId must be provided when AddType is Insert.")
                ]);
            }

            var userId = user.GetUserId();
            Func<Task<ReelElement?>> addAction = request.AddType switch
            {
                AddType.Append => () =>
                    reelService.AppendReelElementAsync(reelId, request.ToReelElement(), userId, cancellationToken),
                AddType.Prepend => () =>
                    reelService.PrependReelElementAsync(reelId, request.ToReelElement(), userId, cancellationToken),
                AddType.Insert => () =>
                    reelService.InsertReelElementAsync(reelId, request.InsertAfterId!.Value, request.ToReelElement(),
                        userId, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(request))
            };

            ReelElement? result = await addAction();
            if (result is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.CreatedAtRoute(result.ToResponse(), nameof(GetReelElement),
                new { reelId, elementId = result.Id });
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Forbid();
        }
    }

    private static async Task<Results<NoContent, NotFound, ForbidHttpResult>> UpdateReelElement
    (
        [FromRoute] Guid reelId,
        [FromRoute] Guid elementId,
        [FromBody] UpdateReelElementRequest request,
        [FromServices] IReelService reelService,
        [FromServices] ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        try
        {
            ReelElement? result =
                await reelService.UpdateReelElementAsync(reelId, request.ToReelElement(), user.GetUserId(),
                    cancellationToken);
            if (result is null)
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

    private static async Task<Results<NoContent, NotFound, ForbidHttpResult>> DeleteReelElement
    (
        [FromRoute] Guid reelId,
        [FromRoute] Guid elementId,
        [FromServices] IReelService reelService,
        [FromServices] ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        try
        {
            bool result =
                await reelService.DeleteReelElementAsync(reelId, elementId, user.GetUserId(), cancellationToken);
            if (!result)
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
}