using System.Security.Cryptography;

namespace Doner.Features.AuthFeature.Services;

public static class RefreshTokenGenerator
{
    public static string GenerateRefreshToken()
    {
        var refreshToken = RandomNumberGenerator.GetHexString(32);
        return refreshToken;
    }
}