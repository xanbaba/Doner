using Doner.Features;
using Microsoft.EntityFrameworkCore;

namespace Doner;

public static class WebApplicationExtensions
{
    public static void AddFeature<TFeature>(this WebApplicationBuilder builder) where TFeature : IFeature
    {
        TFeature.Build(builder);
    }

    public static void UseFeature<TFeature>(this WebApplication app) where TFeature : IFeature
    {
        TFeature.Configure(app);
    }

    public static void MapEndpoints<TEndpointMapper>(this IEndpointRouteBuilder builder) where TEndpointMapper : IEndpointMapper
    {
        TEndpointMapper.Map(builder);
    }

    public static void MigrateDatabase<TDbContext>(this WebApplication app) where TDbContext: DbContext
    {
        app.Services.CreateScope().ServiceProvider.GetRequiredService<TDbContext>().Database.Migrate();
    }
}