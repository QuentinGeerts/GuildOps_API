namespace GuildOps.Domain.Games;

public class CharacterClass
{
    public CharacterClass(Guid gameId, string name)
    {
        Id = Guid.CreateVersion7();
        GameId = gameId;
        Name = name;
    }

    public Guid Id { get; private set; }

    public Guid GameId { get; set; }
    public Game Game { get; set; }

    public string Name { get; set; } = null!;
}
