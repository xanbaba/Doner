using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Doner.Features.AuthFeature;

public class AuthFeature : IFeature
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
    }

    public static void Configure(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        
    }
}