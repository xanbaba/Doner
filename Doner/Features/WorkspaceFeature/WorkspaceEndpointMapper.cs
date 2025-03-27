using System.Security.Claims;
using Contracts.V1.Requests;
using Contracts.V1.Responses;
using Doner.Features.WorkspaceFeature.Exceptions;
using Doner.Features.WorkspaceFeature.Services;
using Doner.Features.WorkspaceFeature.Services.WorkspaceService;
using Doner.Localizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Doner.Features.WorkspaceFeature;

public abstract class WorkspaceEndpointMapper : IEndpointMapper
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/users/me/workspaces", GetByOwnerAsync);

        var workspacesGroup = builder.MapGroup("/me/workspaces").RequireAuthorization();

        workspacesGroup.MapGet("/{id:guid}", GetWorkspace);
        workspacesGroup.MapPost("/", AddWorkspace);
        workspacesGroup.MapPut("/{id:guid}", UpdateWorkspace);
        workspacesGroup.MapDelete("/{id:guid}", RemoveWorkspace);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="workspaceService"></param>
    /// <param name="user"></param>
    /// <param name="localizer"></param>
    /// <returns>All workspaces</returns>
    private static async Task<Ok<WorkspacesResponse>> GetByOwnerAsync(
        [FromServices] IWorkspaceService workspaceService,
        [FromServices] IStringLocalizer<Messages> localizer,
        ClaimsPrincipal user
    )
    {
        var workspacesResult = await workspaceService.GetByOwnerAsync(user.GetUserId());

        return workspacesResult.Match(
            items => TypedResults.Ok(items.ToResponse()),
            exception => throw exception);
    }

    /// <summary>
    /// Not found if workspace with provided id is not found
    /// </summary>
    /// <param name="workspaceService"></param>
    /// <param name="localizer"></param>
    /// <param name="id"></param>
    /// <returns>Workspace json</returns>
    private static async Task<Results<NotFound<string>, Ok<WorkspaceResponse>>> GetWorkspace(
        [FromServices] IWorkspaceService workspaceService,
        [FromServices] IStringLocalizer<Messages> localizer,
        [FromRoute] Guid id
    )
    {
        var workspaceResult = await workspaceService.GetAsync(id);

        return workspaceResult
            .Match<Results<NotFound<string>, Ok<WorkspaceResponse>>>(
                workspace => TypedResults.Ok(workspace.ToResponse()),
                exception => TypedResults.NotFound(exception.Message));
    }

    private static async Task<Results<BadRequest<string>, NotFound<string>, Created>> AddWorkspace(
        [FromServices] IWorkspaceService workspaceService,
        [FromServices] IStringLocalizer<Messages> localizer,
        [FromBody] AddWorkspaceRequest request,
        ClaimsPrincipal user
    )
    {
        var workspace = request.ToWorkspace(user.GetUserId());

        var workspaceResult = await workspaceService.CreateAsync(workspace);

        return workspaceResult.Match<Results<BadRequest<string>, NotFound<string>, Created>>(
            workspaceId => TypedResults.Created(workspaceId.ToString()),
            exception => TypedResults.NotFound(exception.Message)
        );
    }

    private static async Task<Results<NoContent, BadRequest<string>, NotFound<string>>> UpdateWorkspace
    (
        [FromServices] IWorkspaceService workspaceService,
        [FromServices] IStringLocalizer<Messages> localizer,
        [FromBody] UpdateWorkspaceRequest request,
        ClaimsPrincipal user,
        Guid id
    )
    {
        var workspace = request.ToWorkspace(id, user.GetUserId());
        var workspaceResult = await workspaceService.UpdateAsync(workspace);

        return workspaceResult.Match<Results<NoContent, BadRequest<string>, NotFound<string>>>(
            _ => TypedResults.NoContent(),
            exception =>
            {
                return exception switch
                {
                    WorkspaceNotFoundException => TypedResults.NotFound(exception.Message),
                    PermissionDeniedException => TypedResults.BadRequest(exception.Message),
                    _ => throw exception
                };
            }
        );
    }

    /// <summary>
    /// Not found if workspace with provided id is not found
    /// </summary>
    /// <param name="workspaceService"></param>
    /// <param name="id"></param>
    /// <param name="user"></param>
    /// <returns>error, ok</returns>
    private static async Task<Results<NotFound<string>, NoContent, BadRequest<string>>> RemoveWorkspace(
        [FromServices] IWorkspaceService workspaceService,
        [FromRoute] Guid id,
        ClaimsPrincipal user
    )
    {
        var workspaceResult = await workspaceService.RemoveAsync(user.GetUserId(), id);

        return workspaceResult.Match<Results<NotFound<string>, NoContent, BadRequest<string>>>(
            _ => TypedResults.NoContent(),
            exception =>
            {
                return exception switch
                {
                    WorkspaceNotFoundException => TypedResults.NotFound(exception.Message),
                    PermissionDeniedException => TypedResults.BadRequest(exception.Message),
                    _ => throw exception
                };
            }
        );
    }
}