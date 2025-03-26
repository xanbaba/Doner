using Contracts.V1.Responses;
using Doner.Features.WorkspaceFeature.Entities;

namespace Doner.Features.WorkspaceFeature.Services;

public static class WorkspaceMapper
{
    public static WorkspaceResponse ToResponse(this Workspace workspace)
    {
        return new WorkspaceResponse()
        {
            Id = workspace.Id,
            Name = workspace.Name,
            Description = workspace.Description,
            OwnerId = workspace.OwnerId,
            IsArchived = workspace.IsArchived
        };
    }

    public static WorkspacesResponse ToResponse(this IEnumerable<Workspace> workspaces)
    {
        return new WorkspacesResponse()
        {
            Items = workspaces.Select(ToResponse)
        };
    }
}