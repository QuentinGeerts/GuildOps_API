namespace GuildOps.Domain.Players;

public sealed class Player
{
    private Player() { }   // requis par EF Core

    public Player(string pseudo, string email, string identityUserId)
    {
        Id = Guid.CreateVersion7();
        Pseudo = pseudo;
        Email = email;
        IdentityUserId = identityUserId;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }
    public string Pseudo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string IdentityUserId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Character> Characters { get; set; } = [];
}
