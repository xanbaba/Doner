using Contracts.V1.Requests;
using Contracts.V1.Responses;
using Doner.Features.WorkspaceFeature.Entities;

namespace Doner.Features.WorkspaceFeature.Services;

public static class WorkspaceMapper
{
    public static WorkspaceResponse ToResponse(this Workspace workspace) =>
        new()
        {
            Id = workspace.Id,
            Name = workspace.Name,
            Description = workspace.Description,
            OwnerId = workspace.OwnerId
        };

    public static WorkspacesResponse ToResponse(this IEnumerable<Workspace> workspaces) =>
        new()
        {
            Items = workspaces.Select(ToResponse)
        };
    
    public static Workspace ToWorkspace(this AddWorkspaceRequest request, Guid ownerId) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name,
            Description = request.Description,
            OwnerId = ownerId
        };
    
    public static Workspace ToWorkspace(this UpdateWorkspaceRequest request, Guid workspaceId, Guid ownerId) =>
        new()
        {
            Id = workspaceId,
            Name = request.Name,
            Description = request.Description,
            OwnerId = ownerId
        };
}