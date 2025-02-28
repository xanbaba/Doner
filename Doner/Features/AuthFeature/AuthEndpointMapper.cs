using System.Diagnostics.CodeAnalysis;
using Doner.DataBase;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Doner.Features.AuthFeature;

public class AuthEndpointMapper : IEndpointMapper
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/sign-in", SignIn);
        builder.MapGet("/sign-up", () => { });
        builder.MapGet("/refresh", () => { });
    }
    
    [SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local")]
    private record TokensResponse(string AccessToken, string RefreshToken);
    private record LoginRequest(string Login, string Password);

    private static async Task<Results<UnauthorizedHttpResult, Ok<TokensResponse>>> SignIn
    (
        [FromBody] LoginRequest request,
        [FromServices] IConfiguration configuration,
        [FromServices] JwtTokenGenerator accessTokenGenerator,
        [FromServices] IRefreshTokensManager refreshTokensManager,
        [FromServices] AppDbContext dbContext
    )
    {
        var user = dbContext.Users.FirstOrDefault(x => x.Login == request.Login);
        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        var hashedPassword = PasswordHasher.HashPassword(request.Password, user.PasswordSalt);

        if (user.PasswordHash != hashedPassword)
        {
            return TypedResults.Unauthorized();
        }


        var accessToken = accessTokenGenerator.GenerateJwtToken(user);
        var refreshToken = await refreshTokensManager.AssignRefreshTokenAsync(user);
        return TypedResults.Ok(new TokensResponse(accessToken, refreshToken));
    }
}