using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal sealed class AssignMemberRankCommandHandler(IGuildRepository guilds, IUnitOfWork unitOfWork)
    : ICommandHandler<AssignMemberRankCommand, AssignMemberRankOutcome>
{
    public async Task<AssignMemberRankOutcome> HandleAsync(AssignMemberRankCommand command, CancellationToken cancellationToken = default)
    {
        if (!await guilds.HasPermissionAsync(command.GuildId, command.PlayerId, GuildPermission.AssignRank, cancellationToken))
        {
            return AssignMemberRankOutcome.Forbidden;
        }

        var rank = await guilds.GetRankAsync(command.GuildId, command.RankId, cancellationToken);
        if (rank is null)
        {
            return AssignMemberRankOutcome.RankNotInGuild;
        }

        if (rank.IsLeader)
        {
            return AssignMemberRankOutcome.CannotAssignLeaderRank;
        }

        var membership = await guilds.GetMembershipAsync(command.GuildId, command.CharacterId, cancellationToken);
        if (membership is null)
        {
            return AssignMemberRankOutcome.MembershipNotFound;
        }

        var currentRank = await guilds.GetRankAsync(command.GuildId, membership.GuildRankId, cancellationToken);
        if (currentRank is not null && currentRank.IsLeader)
        {
            return AssignMemberRankOutcome.CannotDemoteLeader;
        }

        membership.GuildRankId = rank.Id;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AssignMemberRankOutcome.Updated;
    }
}
