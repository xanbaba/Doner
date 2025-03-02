namespace Doner.Features.AuthFeature;

public static class ConfigurationExtensions
{
    private static T GetJwtOption<T>(IConfiguration configuration, string optionName)
    {
        var jwtOption = configuration.GetSection($"Jwt:{optionName}").Get<T>();
        if (jwtOption is null)
        {
            throw new ApplicationException(
                "Missing JWT options. Try specify JWT options in appsettings.json.\n" +
                "Path must be Jwt:<option>.\n" +
                "Options are [Secret, Issuer, Audience, LifetimeMinutes]");
        }

        return jwtOption;
    }

    public static string GetJwtSecret(this IConfiguration configuration)
    {
        var secret = GetJwtOption<string>(configuration, "Secret");
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ApplicationException("Missing JWT secret. Try specify JWT options in appsettings.json.\n" +
                                           "Path must be Jwt:<option>.\n" +
                                           "Options are [Secret, Issuer, Audience, LifetimeMinutes]");
        }

        return secret;
    }
    
    public static string GetJwtAudience(this IConfiguration configuration)
    {
        var audience = GetJwtOption<string>(configuration, "Audience");
        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new ApplicationException("Missing JWT audience. Try specify JWT options in appsettings.json.\n" +
                                           "Path must be Jwt:<option>.\n" +
                                           "Options are [Secret, Issuer, Audience, LifetimeMinutes]");
        }

        return audience;
    }
    
    public static string GetJwtIssuer(this IConfiguration configuration)
    {
        var issuer = GetJwtOption<string>(configuration, "Issuer");
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new ApplicationException("Missing JWT issuer. Try specify JWT options in appsettings.json.\n" +
                                           "Path must be Jwt:<option>.\n" +
                                           "Options are [Secret, Issuer, Audience, LifetimeMinutes]");
        }

        return issuer;
    }

    public static int GetLifetimeMinutes(this IConfiguration configuration)
    {
        var lifetime = GetJwtOption<int>(configuration, "LifetimeMinutes");
        if (lifetime <= 0)
        {
            throw new ApplicationException("Missing JWT lifetime. Try specify JWT options in appsettings.json.\n" +
                                           "Path must be Jwt:<option>.\n" +
                                           "Options are [Secret, Issuer, Audience, LifetimeMinutes]");
        }

        return lifetime;
    }
}