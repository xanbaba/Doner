using System.Security.Cryptography;

namespace Doner.Features.AuthFeature;

public static class RefreshTokenGenerator
{
    public static string GenerateRefreshToken()
    {
        var refreshToken = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(refreshToken);
    }
}