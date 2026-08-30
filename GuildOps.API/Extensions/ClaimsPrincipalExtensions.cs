using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GuildOps.API.Extensions;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetPlayerId(this ClaimsPrincipal principal)
    {
        string? subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(subject, out Guid playerId)
            ? playerId
            : throw new InvalidOperationException("Le jeton ne porte pas de claim 'sub' exploitable.");
    }
}
