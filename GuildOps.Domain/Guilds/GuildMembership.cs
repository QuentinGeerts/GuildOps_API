using GuildOps.Domain.Players;

namespace GuildOps.Domain.Guilds;

public class GuildMembership
{
    private GuildMembership() { }

    public GuildMembership(Guid guildId, Guid characterId, Guid guildRankId)
    {
        Id = Guid.CreateVersion7();
        JoinedAt = DateTimeOffset.UtcNow;
        GuildId = guildId;
        CharacterId = characterId;
        GuildRankId = guildRankId;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }

    public Guid GuildId { get; set; }
    public Guild? Guild { get; set; }

    public Guid CharacterId { get; set; }
    public Character? Character { get; set; }

    public Guid GuildRankId { get; set; }
    public GuildRank? Rank { get; set; }

    public string? Note { get; set; }
}