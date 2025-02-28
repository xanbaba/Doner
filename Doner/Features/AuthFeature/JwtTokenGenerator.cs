using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Doner.Features.AuthFeature;

public class JwtTokenGenerator(IConfiguration configuration)
{
    public string GenerateJwtToken(User user)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        ];

        var issuer = configuration.GetJwtIssuer();
        var audience = configuration.GetJwtAudience();
        var expires = DateTime.UtcNow.AddMinutes(configuration.GetLifetimeMinutes());
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetJwtSecret()));
        var securityToken = new JwtSecurityToken(issuer, audience, claims, expires: expires,
            signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));
        var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

        return token;
    }

    /*public string GenerateJwtToken(string refreshToken)
    {
        var tokenEntity = dbContext.RefreshTokens.Include(x => x.User).FirstOrDefault(x => x.Token == refreshToken);

        if (tokenEntity is null)
        {
            throw new RefreshTokenNotFoundException();
        }
        
        var user = tokenEntity.User;
        
        return GenerateJwtToken(user);
    }*/
}