using System.Diagnostics.CodeAnalysis;
using Doner.DataBase;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Doner.Features.AuthFeature;

public class AuthEndpointMapper : IEndpointMapper
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/sign-in", SignIn);
        builder.MapPost("/sign-up", () => { });
        builder.MapPost("/refresh", Refresh);
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

        if (!PasswordHasher.ConstantTimeCompare(hashedPassword, user.PasswordHash))
        {
            return TypedResults.Unauthorized();
        }


        var accessToken = accessTokenGenerator.GenerateJwtToken(user);
        return (await refreshTokensManager.AssignRefreshTokenAsync(user))
            .Match<Results<UnauthorizedHttpResult, Ok<TokensResponse>>>(
                refreshToken => TypedResults.Ok(new TokensResponse(accessToken, refreshToken)),
                _ => TypedResults.Unauthorized());
    }

    private record RefreshRequest(string RefreshToken);

    private static async Task<Results<Ok<TokensResponse>, UnauthorizedHttpResult>> Refresh
    (
        [FromBody] RefreshRequest request,
        [FromServices] IRefreshTokensManager refreshTokensManager,
        [FromServices] JwtTokenGenerator accessTokenGenerator
    )
    {
        return (await refreshTokensManager.RefreshTokenAsync(request.RefreshToken))
            .Bind(refreshToken => accessTokenGenerator.GenerateJwtToken(refreshToken)
                .Map(jwtToken => new TokensResponse(jwtToken, refreshToken)))
            .Match<Results<Ok<TokensResponse>, UnauthorizedHttpResult>>(x => TypedResults.Ok(x),
                _ => TypedResults.Unauthorized());
    }
}