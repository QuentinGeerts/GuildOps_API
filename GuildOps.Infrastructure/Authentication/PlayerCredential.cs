using GuildOps.Domain.Players;

namespace GuildOps.Infrastructure.Authentication;

public sealed class PlayerCredential
{
    private PlayerCredential() { }

    public PlayerCredential(Guid playerId, string email, string passwordHash)
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.UtcNow;
        PlayerId = playerId;
        Email = email;
        PasswordHash = passwordHash;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }

    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
}
