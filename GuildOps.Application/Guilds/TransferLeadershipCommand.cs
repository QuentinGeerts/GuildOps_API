namespace GuildOps.Application.Guilds;

public sealed record TransferLeadershipCommand(Guid PlayerId, Guid GuildId, Guid CharacterId);

public enum TransferLeadershipOutcome
{
    Transferred = 1,
    NotLeader = 2,
    MembershipNotFound = 3,
    AlreadyLeader = 4
}
