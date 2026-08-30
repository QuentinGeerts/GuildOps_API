using System.ComponentModel.DataAnnotations;

namespace GuildOps.Application.Guilds;

public sealed record AssignMemberRankRequest([Required] Guid RankId);

public sealed record AssignMemberRankCommand(Guid PlayerId, Guid GuildId, Guid CharacterId, Guid RankId)
{
    public static AssignMemberRankCommand From(Guid playerId, Guid guildId, Guid characterId, AssignMemberRankRequest request)
        => new(playerId, guildId, characterId, request.RankId);
}

public enum AssignMemberRankOutcome
{
    Updated = 1,
    Forbidden = 2,
    MembershipNotFound = 3,
    RankNotInGuild = 4,
    CannotAssignLeaderRank = 5,
    CannotDemoteLeader = 6
}
