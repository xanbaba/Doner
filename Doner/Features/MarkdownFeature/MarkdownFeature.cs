namespace Doner.Features.MarkdownFeature;

public class MarkdownFeature : IFeature
{
    public static void Build(WebApplicationBuilder builder)
    {
        
        // Register OT processor as singleton since it's stateless
        builder.Services.AddSingleton<IOTProcessor, OTProcessor>();
        // Register OT service as scoped or transient based on your requirements
        builder.Services.AddScoped<IOTService, OTService>();
    }

    public static void Configure(WebApplication app)
    {
        
    }
}