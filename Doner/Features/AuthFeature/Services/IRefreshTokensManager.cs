using Doner.Features.AuthFeature.Entities;
using LanguageExt.Common;

namespace Doner.Features.AuthFeature.Services;

public interface IRefreshTokensManager
{
    public Task<Result<string>> RefreshTokenAsync(string refreshToken);
    public Task<Result<string>> AssignRefreshTokenAsync(User user);
}