using GuildOps.Domain.Games;

namespace GuildOps.Domain.Players;

public class CharacterGameRole
{
    private CharacterGameRole() { }

    public CharacterGameRole(Guid characterId, Guid gameRoleId)
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.UtcNow;
        CharacterId = characterId;
        GameRoleId = gameRoleId;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CharacterId { get; set; }
    public Character? Character { get; set; }

    public Guid GameRoleId { get; set; }
    public GameRole? GameRole { get; set; }
}
