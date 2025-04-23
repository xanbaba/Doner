using Doner.Features.MarkdownFeature.Locking;
using Doner.Features.MarkdownFeature.Redis;
using StackExchange.Redis;

namespace Doner.Features.MarkdownFeature;

public class MarkdownFeature : IFeature
{
    public static void Build(WebApplicationBuilder builder)
    {
        // Register options
        builder.Services.Configure<RedisOptions>(
            builder.Configuration.GetSection("Redis"));
        
        // Register Redis connection multiplexer as singleton
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var config = builder.Configuration.GetSection("Redis:ConnectionString").Value!;
            return ConnectionMultiplexer.Connect(config);
        });
        
        // Register Redis implementations
        builder.Services.AddSingleton<ISessionManager, RedisSessionManager>();
        builder.Services.AddSingleton<IOperationRepository, RedisOperationRepository>();
        
        // Register the Redis-based distributed lock manager
        builder.Services.AddSingleton<IDistributedLockManager, RedisDistributedLockManager>();
        
        // Register Redis cleanup background service
        builder.Services.AddHostedService<RedisCleanupService>();
        
        // Register OT processor as singleton since it's stateless
        builder.Services.AddSingleton<IOTProcessor, OTProcessor>();
        
        // Register OT service as scoped or transient based on your requirements
        builder.Services.AddScoped<IOTService, OTService>();
    }

    public static void Configure(WebApplication app)
    {
        
    }
}