using Doner.Features.ReelsFeature.Repository;
using Doner.Features.ReelsFeature.Services;
using FluentValidation;
using MongoDB.Driver;

namespace Doner.Features.ReelsFeature;

public class ReelsFeature: IFeature
{
    public static void Build(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IMongoCollection<Reel>>(sp =>
        {
            var db = sp.GetRequiredService<IMongoDatabase>();
            return db.GetCollection<Reel>("Reels");
        });

        builder.Services.AddTransient<IReelRepository, ReelRepository>();
        builder.Services.AddTransient<IReelService, ReelService>();
        builder.Services.AddScoped<IValidator<Reel>, ReelValidator>();
    }

    public static void Configure(WebApplication app)
    {
        
    }
}