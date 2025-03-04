using System.Text;
using Doner.Features.AuthFeature.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Doner.Features.AuthFeature;

public abstract class AuthFeature : IFeature
{
    public static void Build(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration.GetJwtIssuer(),

                ValidateAudience = true,
                ValidAudience = builder.Configuration.GetJwtAudience(),

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetJwtSecret())),

                ValidateLifetime = true
            };
        });

        builder.Services.AddAuthorization();
        builder.Services.AddScoped<IRefreshTokensManager, RefreshTokensManager>();
        builder.Services.AddScoped<JwtTokenGenerator>();
    }

    public static void Configure(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        
        var group = app.MapGroup("/api/v1");
        group.MapEndpoints<AuthEndpointMapper>();
    }
}