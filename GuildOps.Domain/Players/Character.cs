using GuildOps.Domain.Games;
using GuildOps.Domain.Guilds;

namespace GuildOps.Domain.Players;

public sealed class Character
{
    private Character() { }

    public Character(Guid playerId, Guid gameId, Guid characterClassId, string name, string server, int level = 1)
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.UtcNow;
        PlayerId = playerId;
        GameId = gameId;
        CharacterClassId = characterClassId;
        Name = name;
        Server = server;
        Level = level;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    
    public Guid PlayerId { get; set; }
    public Player? Player { get; set; }
    
    public Guid CharacterClassId { get; set; }
    public CharacterClass? CharacterClass { get; set; }

    public Guid GameId { get; set; }
    public Game? Game { get; set; }

    public string Server { get; set; } = null!; 
    public string Name { get; set; } = null!;
    public int Level { get; set; }

    public GuildMembership? Membership{ get; set; }

    public ICollection<CharacterGameRole> Roles { get; set; } = [];
    public ICollection<Availability> Availabilities { get; set; } = [];
}
