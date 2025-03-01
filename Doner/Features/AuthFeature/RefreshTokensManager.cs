using Doner.DataBase;
using LanguageExt.Common;

namespace Doner.Features.AuthFeature;

public class RefreshTokensManager(AppDbContext dbContext) : IRefreshTokensManager
{
    // ToDo: Move this to appsettings.json
    private const int RefreshTokenLifetimeDays = 30;
    
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
        existingRefreshToken.UtcExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenLifetimeDays);
        await dbContext.SaveChangesAsync();
        return newRefreshToken;
    }

    public async Task<Result<string>> AssignRefreshTokenAsync(User user)
    {
        var token = RefreshTokenGenerator.GenerateRefreshToken();
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            UtcExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenLifetimeDays),
            Token = token
        });
        
        await dbContext.SaveChangesAsync();

        return token;
    }
}