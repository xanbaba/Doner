using System.Security.Claims;
using Contracts.V1.Requests;
using Contracts.V1.Responses;
using Doner.Features.WorkspaceFeature.Exceptions;
using Doner.Features.WorkspaceFeature.Services;
using Doner.Features.WorkspaceFeature.Services.WorkspaceService;
using Doner.Resources;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Doner.Features.WorkspaceFeature;

public abstract class WorkspaceEndpointMapper: IEndpointMapper
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        var workspacesGroup = builder.MapGroup("/users/me/workspaces").RequireAuthorization();

        workspacesGroup.MapGet("/", GetByOwnerAsync);
        workspacesGroup.MapGet("/{id:guid}", GetWorkspace);
        workspacesGroup.MapPost("/", AddWorkspace);
        workspacesGroup.MapPut("/", UpdateWorkspace);
        workspacesGroup.MapDelete("/{id:guid}", RemoveWorkspace);
    }


    private static async Task<Ok<WorkspacesResponse>> GetByOwnerAsync(
        [FromServices] IWorkspaceService workspaceService,
        ClaimsPrincipal user
    )
    {
        var workspacesResult = await workspaceService.GetByOwnerAsync(user.GetUserId());
        
        return workspacesResult.Match(
            items => TypedResults.Ok(items.ToResponse()),
            exception => throw exception);
    }

    private static async Task<Results<NotFound<string>, Ok<WorkspaceResponse>>> GetWorkspace(
        [FromServices] IWorkspaceService workspaceService,
        [FromServices] IStringLocalizer<WorkspaceEndpointMapper> localizer,
        [FromRoute] Guid id
        )
    {
        var workspaceResult = await workspaceService.GetAsync(id);
        
        return workspaceResult
            .Match<Results<NotFound<string>, Ok<WorkspaceResponse>>>(
                workspace => TypedResults.Ok(workspace.ToResponse()), 
                exception => exception switch
                {
                    WorkspaceNotFoundException => TypedResults.NotFound(localizer["WorkspaceNotFound"].Value),
                    _ => throw exception
                });
    }
    
    private static async Task<Results<BadRequest<string>, NotFound<string>, Created>> AddWorkspace(
        [FromServices] IWorkspaceService workspaceService,
        [FromServices] IStringLocalizer<WorkspaceEndpointMapper> localizer,
        [FromBody] AddWorkspaceRequest request,
        ClaimsPrincipal user
        )
    {
        var workspace = request.ToWorkspace(user.GetUserId());
        
        workspace.OwnerId = user.GetUserId();
        
        var workspaceResult = await workspaceService.CreateAsync(workspace);

        return workspaceResult.Match<Results<BadRequest<string>, NotFound<string>, Created>>(
            workspaceId => TypedResults.Created(workspaceId.ToString()),
            exception => exception switch
            {
                WorkspaceNameRequiredException => TypedResults.BadRequest(localizer["WorkspaceNameRequired"].Value),
                WorkspaceAlreadyExistsException => TypedResults.BadRequest(localizer["WorkspaceAlreadyExists"].Value),
                _ => throw exception
            }
        );
    }

    private static async Task<Results<NoContent, BadRequest<string>, NotFound<string>>> UpdateWorkspace(
        [FromServices] IWorkspaceService workspaceService,
        [FromServices] IStringLocalizer<WorkspaceEndpointMapper> localizer,
        [FromServices] IStringLocalizer<SharedResources> sharedLocalizer,
        [FromBody] UpdateWorkspaceRequest request,
        ClaimsPrincipal user,
        Guid id
        )
    {
        var workspace = request.ToWorkspace(id, user.GetUserId());
        var workspaceResult = await workspaceService.UpdateAsync(workspace);

        return workspaceResult.Match<Results<NoContent, BadRequest<string>, NotFound<string>>>(
            _ => TypedResults.NoContent(),
            exception => exception switch
            {
                WorkspaceNotFoundException => TypedResults.NotFound(localizer["WorkspaceNotFound"].Value),
                PermissionDeniedException => TypedResults.BadRequest(sharedLocalizer["PermissionDenied"].Value),
                WorkspaceAlreadyExistsException => TypedResults.BadRequest(localizer["WorkspaceAlreadyExists"].Value),
                _ => throw exception
            });
    }
    
    private static async Task<Results<NotFound<string>, NoContent, BadRequest<string>>> RemoveWorkspace(
        [FromServices] IWorkspaceService workspaceService,
        [FromServices] IStringLocalizer<WorkspaceEndpointMapper> localizer,
        [FromServices] IStringLocalizer<SharedResources> sharedLocalizer,
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
                    WorkspaceNotFoundException => TypedResults.NotFound(localizer["WorkspaceNotFound"].Value),
                    PermissionDeniedException => TypedResults.BadRequest(sharedLocalizer["PermissionDenied"].Value),
                    _ => throw exception
                };
            }
        );
    }
}