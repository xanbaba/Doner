using Doner.DataBase;
using Doner.Features.AuthFeature.Entities;
using Doner.Features.AuthFeature.Exceptions;
using LanguageExt.Common;

namespace Doner.Features.AuthFeature.Services;

public class RefreshTokensManager(AppDbContext dbContext, IConfiguration configuration) : IRefreshTokensManager
{
    private readonly int _refreshTokenLifetimeDays = configuration.GetValue<int>("RefreshTokenLifetimeDays");
    
    public async Task<Result<string>> RefreshTokenAsync(string refreshToken)
    {
        var existingRefreshToken = dbContext.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
        if (existingRefreshToken == null)
        {
            return new Result<string>(new RefreshTokenNotFoundException());
        }

        if (existingRefreshToken.UtcExpiresAt < DateTime.UtcNow)
        {
            return new Result<string>(new RefreshTokenExpiredException());
        }

        var newRefreshToken = RefreshTokenGenerator.GenerateRefreshToken();
        
        existingRefreshToken.Token = newRefreshToken;
        existingRefreshToken.UtcExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenLifetimeDays);
        await dbContext.SaveChangesAsync();
        return newRefreshToken;
    }

    public async Task<Result<string>> AssignRefreshTokenAsync(User user)
    {
        var token = RefreshTokenGenerator.GenerateRefreshToken();
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            UtcExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenLifetimeDays),
            Token = token
        });
        
        await dbContext.SaveChangesAsync();

        return token;
    }
}