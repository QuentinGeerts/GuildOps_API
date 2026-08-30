namespace GuildOps.Application.Abstractions;

/// <summary>La valeur en clair d'un jeton de rafraichissement : elle n'existe qu'a l'emission.</summary>
public sealed record IssuedRefreshToken(string Value, DateTimeOffset ExpiresAt);

public interface IRefreshTokenStore
{
    IssuedRefreshToken Create(Guid playerId);

    Task<Guid?> ConsumeAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);
}
