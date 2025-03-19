using Doner.Features.WorkspaceFeature.Repository;
using Doner.Features.WorkspaceFeature.Service;

namespace Doner.Features.WorkspaceFeature;

public class WorkspaceFeature: IFeature
{
    public static void Build(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
    }

    public static void Configure(WebApplication app)
    {
        var group = app.MapGroup("api/v1");
        group.MapEndpoints<WorkspaceEndpointMapper>();
    }
}