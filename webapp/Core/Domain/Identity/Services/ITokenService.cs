using System.Security.Claims;

namespace TarlBreuJacoBaraKnor.Core.Domain.Identity.Services;

public interface ITokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
}
