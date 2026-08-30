namespace GuildOps.Application.Guilds;

public sealed record KickMemberCommand(Guid PlayerId, Guid GuildId, Guid CharacterId);

public enum KickMemberOutcome
{
    Kicked = 1,
    Forbidden = 2,
    MembershipNotFound = 3,
    CannotKickLeader = 4
}
