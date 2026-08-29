using GuildOps.Domain.Games;
using GuildOps.Domain.Guilds;

namespace GuildOps.Domain.Players;

public sealed class Character
{
    public Character() { }
    public Character(Guid id, Guid playerId, string server, string name, Guid characterClassId, int level)
    {
        Id = Guid.CreateVersion7();
        PlayerId = playerId;
        Server = server;
        Name = name;
        CharacterClassId = characterClassId;
        Level = level;
        CreatedAt = DateTime.UtcNow;
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
}
