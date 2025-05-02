using System.Security.Claims;
using Contracts.V1.Requests;
using Contracts.V1.Responses;
using Doner.Features.WorkspaceFeature.Exceptions;
using Doner.Features.WorkspaceFeature.Services;
using Doner.Features.WorkspaceFeature.Services.WorkspaceService;
using Doner.Resources;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Doner.Features.WorkspaceFeature;

public abstract class WorkspaceEndpointMapper: IEndpointMapper
{
    private const string WorkspaceNotFound = "Workspace does not exist.";
    private const string WorkspaceNameRequired = "Workspace name is required.";
    private const string WorkspaceAlreadyExists = "A workspace with this name already exists.";
    
    public static void Map(IEndpointRouteBuilder builder)
    {
        var workspacesGroup = builder.MapGroup("/users/me/workspaces").RequireAuthorization();

        workspacesGroup.MapGet("/", GetByOwnerAsync);
        workspacesGroup.MapGet("/{id:guid}", GetWorkspace).WithName(nameof(GetWorkspace));
        workspacesGroup.MapPost("/", AddWorkspace);
        workspacesGroup.MapPut("/{id:guid}", UpdateWorkspace);
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
        [FromRoute] Guid id,
        [FromServices] ClaimsPrincipal user
        )
    {
        var workspaceResult = await workspaceService.GetAsync(id, user.GetUserId());
        
        return workspaceResult
            .Match<Results<NotFound<string>, Ok<WorkspaceResponse>>>(
                workspace => TypedResults.Ok(workspace.ToResponse()), 
                exception => exception switch
                {
                    WorkspaceNotFoundException => TypedResults.NotFound(WorkspaceNotFound),
                    _ => throw exception
                });
    }
    
    private static async Task<Results<BadRequest<string>, NotFound<string>, CreatedAtRoute<WorkspaceResponse>>> AddWorkspace(
        [FromServices] IWorkspaceService workspaceService,
        [FromBody] AddWorkspaceRequest request,
        ClaimsPrincipal user
        )
    {
        var workspace = request.ToWorkspace(user.GetUserId());
        
        var workspaceResult = await workspaceService.CreateAsync(workspace);

        return workspaceResult.Match<Results<BadRequest<string>, NotFound<string>, CreatedAtRoute<WorkspaceResponse>>>(
            workspaceId => TypedResults.CreatedAtRoute(workspace.ToResponse(), nameof(GetWorkspace), new { id = workspaceId }),
            exception => exception switch
            {
                WorkspaceNameRequiredException => TypedResults.BadRequest(WorkspaceNameRequired),
                WorkspaceAlreadyExistsException => TypedResults.BadRequest(WorkspaceAlreadyExists),
                _ => throw exception
            }
        );
    }

    private static async Task<Results<NoContent, BadRequest<string>, NotFound<string>>> UpdateWorkspace(
        [FromServices] IWorkspaceService workspaceService,
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
                WorkspaceNotFoundException => TypedResults.NotFound(WorkspaceNotFound),
                PermissionDeniedException => TypedResults.BadRequest(SharedResources.PermissionDenied),
                WorkspaceAlreadyExistsException => TypedResults.BadRequest(WorkspaceAlreadyExists),
                _ => throw exception
            });
    }
    
    private static async Task<Results<NotFound<string>, NoContent, BadRequest<string>>> RemoveWorkspace(
        [FromServices] IWorkspaceService workspaceService,
        [FromRoute] Guid id,
        ClaimsPrincipal user
    )
    {
        var workspaceResult = await workspaceService.RemoveAsync(id, user.GetUserId());
        
        return workspaceResult.Match<Results<NotFound<string>, NoContent, BadRequest<string>>>(
            _ => TypedResults.NoContent(),
            exception =>
            {
                return exception switch
                {
                    WorkspaceNotFoundException => TypedResults.NotFound(WorkspaceNotFound),
                    PermissionDeniedException => TypedResults.BadRequest(SharedResources.PermissionDenied),
                    _ => throw exception
                };
            }
        );
    }
}