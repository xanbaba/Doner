using Doner.Features.WorkspaceFeature.Repository;
using Doner.Features.WorkspaceFeature.Services.EmailService;
using Doner.Features.WorkspaceFeature.Services.InviteLinkService;
using Doner.Features.WorkspaceFeature.Services.WorkspaceService;

namespace Doner.Features.WorkspaceFeature;

public class WorkspaceFeature: IFeature
{
    public static void Build(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IInviteTokenService, InviteTokenService>();
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
    }

    public static void Configure(WebApplication app)
    {
        var group = app.MapGroup("api/v1");
        group.MapEndpoints<WorkspaceEndpointMapper>();
    }
}