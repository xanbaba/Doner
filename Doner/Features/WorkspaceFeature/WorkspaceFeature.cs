using Doner.Features.WorkspaceFeature.Repository;

namespace Doner.Features.WorkspaceFeature;

public class WorkspaceFeature: IFeature
{
    public static void Build(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepositoryLocal>();
    }

    public static void Configure(WebApplication app)
    {
        app.MapEndpoints<WorkspaceEndpointMapper>();
    }
}