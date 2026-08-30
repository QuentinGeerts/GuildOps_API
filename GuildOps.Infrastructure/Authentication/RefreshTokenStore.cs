using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using GuildOps.Application.Abstractions;
using GuildOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GuildOps.Infrastructure.Authentication;

internal sealed class RefreshTokenStore(
    ApplicationDbContext context,
    TimeProvider timeProvider,
    IOptions<JwtOptions> options)
    : IRefreshTokenStore
{
    private const int TokenSizeInBytes = 32;

    public IssuedRefreshToken Create(Guid playerId)
    {
        string value = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenSizeInBytes));
        DateTimeOffset expiresAt = timeProvider.GetUtcNow().AddDays(options.Value.RefreshExpiryDays);

        context.Set<RefreshToken>().Add(new RefreshToken(playerId, Hash(value), expiresAt));

        return new IssuedRefreshToken(value, expiresAt);
    }

    public async Task<Guid?> ConsumeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var stored = await FindAsync(refreshToken, cancellationToken);

        if (stored is null || stored.RevokedAt is not null || stored.ExpiresAt <= timeProvider.GetUtcNow())
        {
            return null;
        }

        // rotation : le jeton presente ne resservira jamais
        stored.RevokedAt = timeProvider.GetUtcNow();

        return stored.PlayerId;
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var stored = await FindAsync(refreshToken, cancellationToken);

        if (stored is not null && stored.RevokedAt is null)
        {
            stored.RevokedAt = timeProvider.GetUtcNow();
        }
    }

    private Task<RefreshToken?> FindAsync(string refreshToken, CancellationToken cancellationToken)
    {
        string hash = Hash(refreshToken);

        return context.Set<RefreshToken>()
            .FirstOrDefaultAsync(token => token.TokenHash == hash, cancellationToken);
    }

    // SHA-256 suffit : la valeur est deja 256 bits d'aleatoire, il n'y a pas de dictionnaire a opposer
    private static string Hash(string value)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
