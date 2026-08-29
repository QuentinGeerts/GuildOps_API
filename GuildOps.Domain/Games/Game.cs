namespace GuildOps.Domain.Games;

public class Game
{
    private Game() { }

    public Game(string name, int maxLevel)
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.UtcNow;
        Name = name;
        MaxLevel = maxLevel;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public string Name { get; set; } = null!;
    public int MaxLevel { get; set; }

    public ICollection<CharacterClass> Classes { get; set; } = [];
}
