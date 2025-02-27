namespace Doner.Features.AuthFeature;

public class AuthEndpointMapper : IEndpointMapper
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/sign-in", () => { });
        builder.MapGet("/sign-up", () => { });
        builder.MapGet("/refresh", () => { });
    }
}