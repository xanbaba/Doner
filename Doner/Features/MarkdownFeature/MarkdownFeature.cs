namespace Doner.Features.MarkdownFeature;

public abstract class MarkdownFeature : IFeature
{
    public static void Build(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IMarkdownService, MarkdownService>();
    }

    public static void Configure(WebApplication app)
    {
        
    }
}