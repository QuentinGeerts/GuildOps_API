using GuildOps.Domain.Players;

namespace GuildOps.Infrastructure.Authentication;

public sealed class RefreshToken
{
    private RefreshToken() { }

    public RefreshToken(Guid playerId, string tokenHash, DateTimeOffset expiresAt)
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.UtcNow;
        PlayerId = playerId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }

    public string TokenHash { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
