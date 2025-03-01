using LanguageExt.Common;

namespace Doner.Features.AuthFeature;

public interface IRefreshTokensManager
{
    public Task<Result<string>> RefreshTokenAsync(string refreshToken);
    public Task<Result<string>> AssignRefreshTokenAsync(User user);
}