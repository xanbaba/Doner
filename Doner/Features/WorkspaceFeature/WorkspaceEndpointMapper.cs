using Doner.Features.WorkspaceFeature.Entities;
using Doner.Features.WorkspaceFeature.Repository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Doner.Features.WorkspaceFeature;

public abstract class WorkspaceEndpointMapper: IEndpointMapper
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetAllWorkspaces);
        builder.MapGet("/{id:guid}", GetWorkspace);
        builder.MapPost("/", AddWorkspace);
        builder.MapDelete("/{id:guid}", RemoveWorkspace);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="workspaceRepository"></param>
    /// <returns>All workspaces</returns>
    private static async Task<JsonHttpResult<IEnumerable<Workspace>>> GetAllWorkspaces(
        [FromServices] IWorkspaceRepository workspaceRepository
    )
    {
        return TypedResults.Json(await workspaceRepository.GetAsync());
    }

    /// <summary>
    /// Not found if workspace with provided id is not found
    /// </summary>
    /// <param name="workspaceRepository"></param>
    /// <param name="id"></param>
    /// <returns>Workspace json</returns>
    private static async Task<Results<NotFound<string>, JsonHttpResult<Workspace>>> GetWorkspace(
        [FromServices] IWorkspaceRepository workspaceRepository,
        [FromRoute] Guid id
        )
    {
        var workspace = await workspaceRepository.GetAsync(id);

        if (workspace is null)
        {
            return TypedResults.NotFound($"Workspace with id {id} was not found");
        }
        
        return TypedResults.Json(workspace);
    }

    /// <summary>
    /// bad request if an invalid workspace json is provided
    /// </summary>
    /// <param name="workspaceRepository"></param>
    /// <param name="workspace"></param>
    /// <returns>Id of just now created workspace</returns>
    private static async Task<Results<BadRequest<string>, Ok<Guid>>> AddWorkspace(
        [FromServices] IWorkspaceRepository workspaceRepository,
        [FromBody] Workspace? workspace
        )
    {
        if (workspace is null)
        {
            return TypedResults.BadRequest("No workspace was provided");
        }
        
        var workspaceId =  await workspaceRepository.AddAsync(workspace);
        
        return TypedResults.Ok(workspaceId);
    }
    
    /// <summary>
    /// Not found if workspace with provided id is not found
    /// </summary>
    /// <param name="workspaceRepository"></param>
    /// <param name="id"></param>
    /// <returns>error, ok</returns>
    private static async Task<Results<NotFound<string>, Ok>> RemoveWorkspace(
        [FromServices] IWorkspaceRepository workspaceRepository,
        [FromRoute] Guid id
    )
    {
        var workspace = await workspaceRepository.GetAsync(id);
        
        if (workspace is null)
        {
            return TypedResults.NotFound($"Workspace with ID {id} was not found.");
        }
        
        await workspaceRepository.RemoveAsync(id);
        
        return TypedResults.Ok();
    }
}