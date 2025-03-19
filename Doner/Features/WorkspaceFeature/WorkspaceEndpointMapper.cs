using System.Security.Claims;
using Doner.Features.WorkspaceFeature.Entities;
using Doner.Features.WorkspaceFeature.Exceptions;
using Doner.Features.WorkspaceFeature.Repository;
using Doner.Features.WorkspaceFeature.Service;
using Doner.Localizer;
using LanguageExt;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Doner.Features.WorkspaceFeature;

public abstract class WorkspaceEndpointMapper: IEndpointMapper
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/users/me/workspaces", GetByOwnerAsync);
        
        var workspacesGroup = builder.MapGroup("/workspaces").RequireAuthorization();
        
        workspacesGroup.MapGet("/{id:guid}", GetWorkspace);
        workspacesGroup.MapPost("/", AddWorkspace);
        workspacesGroup.MapPut("/", UpdateWorkspace);
        workspacesGroup.MapDelete("/{id:guid}", RemoveWorkspace);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="workspaceService"></param>
    /// <param name="user"></param>
    /// <param name="localizer"></param>
    /// <returns>All workspaces</returns>
    private static async Task<Ok<IEnumerable<Workspace>>> GetByOwnerAsync(
        [FromServices] IWorkspaceService workspaceService,
        [FromServices] IStringLocalizer<Messages> localizer,
        ClaimsPrincipal user
    )
    {
        var workspacesResult = await workspaceService.GetByOwnerAsync(user.GetUserId());

        return workspacesResult.Match(
            TypedResults.Ok,
            exception => throw exception);
    }

    /// <summary>
    /// Not found if workspace with provided id is not found
    /// </summary>
    /// <param name="workspaceService"></param>
    /// <param name="localizer"></param>
    /// <param name="id"></param>
    /// <returns>Workspace json</returns>
    private static async Task<Results<NotFound<string>, Ok<Workspace>>> GetWorkspace(
        [FromServices] IWorkspaceService workspaceService,
        [FromServices] IStringLocalizer<Messages> localizer,
        [FromRoute] Guid id
        )
    {
        var workspaceResult = await workspaceService.GetAsync(id);
        
        return workspaceResult
            .Match<Results<NotFound<string>, Ok<Workspace>>>(
                workspace => TypedResults.Ok(workspace), 
                exception => TypedResults.NotFound(exception.Message));
    }

    /// <summary>
    /// bad request if an invalid workspace json is provided
    /// </summary>
    /// <param name="workspaceService"></param>
    /// <param name="localizer"></param>
    /// <param name="workspace"></param>
    /// <param name="user"></param>
    /// <returns>Id of just now created workspace</returns>
    private static async Task<Results<BadRequest<string>, NotFound<string>, Ok<Guid>>> AddWorkspace(
        [FromServices] IWorkspaceService workspaceService,
        [FromServices] IStringLocalizer<Messages> localizer,
        [FromBody] Workspace? workspace,
        ClaimsPrincipal user
        )
    {
        if (workspace is null)
        {
            return TypedResults.BadRequest(localizer["InvalidWorkspace"].Value);
        }
        
        workspace.OwnerId = user.GetUserId();
        
        var workspaceResult = await workspaceService.CreateAsync(workspace);

        return workspaceResult.Match<Results<BadRequest<string>, NotFound<string>, Ok<Guid>>>(
            workspaceId => TypedResults.Ok(workspaceId),
            exception => TypedResults.NotFound(exception.Message)
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="workspaceService"></param>
    /// <param name="localizer"></param>
    /// <param name="workspace"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static async Task<Results<Ok, BadRequest<string>, NotFound<string>>> UpdateWorkspace(
        [FromServices] IWorkspaceService workspaceService,
        [FromServices] IStringLocalizer<Messages> localizer,
        [FromBody] Workspace? workspace,
        ClaimsPrincipal user
        )
    {

        if (workspace is null)
        {
            return TypedResults.BadRequest(localizer["InvalidWorkspace"].Value);
        }
        
        var workspaceResult = await workspaceService.UpdateAsync(user.GetUserId(), workspace);

        return workspaceResult.Match<Results<Ok, BadRequest<string>, NotFound<string>>>(
            _ => TypedResults.Ok(),
            exception =>
            {
                return exception switch
                {
                    (WorkspaceNotFoundException) => TypedResults.NotFound(exception.Message),
                    (PermissionDeniedException) => TypedResults.BadRequest(exception.Message),
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
    private static async Task<Results<NotFound<string>, Ok, BadRequest<string>>> RemoveWorkspace(
        [FromServices] IWorkspaceService workspaceService,
        [FromRoute] Guid id,
        ClaimsPrincipal user
    )
    {
        var workspaceResult = await workspaceService.RemoveAsync(user.GetUserId(), id);
        
        
        return workspaceResult.Match<Results<NotFound<string>, Ok, BadRequest<string>>>(
            _ => TypedResults.Ok(),
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