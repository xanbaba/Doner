namespace Doner.Features.MarkdownFeature;

public abstract class MarkdownFeature : IFeature
{
    public static void Build(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IMarkdownEntityService, MarkdownEntityService>();
    }

    public static void Configure(WebApplication app)
    {
        app.MapGroup("/api/v1").MapEndpoints<MarkdownEndpointMapper>();
    }
}