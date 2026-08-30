using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Players;

public sealed record AuthTokensDto(
    string AccessToken,
    DateTimeOffset AccessExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshExpiresAt)
{
    public static AuthTokensDto From(AccessToken access, IssuedRefreshToken refresh)
        => new(access.Value, access.ExpiresAt, refresh.Value, refresh.ExpiresAt);
}
