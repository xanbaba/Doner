using System.Security.Claims;
using LanguageExt;
using LanguageExt.Common;

namespace Doner;

public static class ClaimsPrincipalExtension
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

        return Guid.Parse(userIdClaim!);
    }
}