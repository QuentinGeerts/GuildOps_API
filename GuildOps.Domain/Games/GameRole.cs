namespace GuildOps.Domain.Games;

public class GameRole
{
    private GameRole() { }

    public GameRole(Guid gameId, string name, int sortOrder)
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.UtcNow;
        GameId = gameId;
        Name = name;
        SortOrder = sortOrder;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Guid GameId { get; set; }
    public Game? Game { get; set; }

    public string Name { get; set; } = null!;
    public int SortOrder { get; set; }
}
