namespace GuildOps.Domain.Players;

public sealed class Player
{
    private Player() { }

    public Player(string accountName)
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.UtcNow;
        AccountName = accountName;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public string AccountName { get; set; } = null!;

    public ICollection<Character> Characters { get; set; } = [];
}
