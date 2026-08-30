using GuildOps.Domain.Players;

namespace GuildOps.Domain.Guilds;

public class GuildInvitation
{
    private GuildInvitation() { }

    public GuildInvitation(Guid guildId, Guid characterId, string? message = null)
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.UtcNow;
        GuildId = guildId;
        CharacterId = characterId;
        Message = message;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Guid GuildId { get; set; }
    public Guild? Guild { get; set; }

    public Guid CharacterId { get; set; }
    public Character? Character { get; set; }

    public string? Message { get; set; }
}
