using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal sealed class KickMemberCommandHandler(IGuildRepository guilds, IUnitOfWork unitOfWork)
    : ICommandHandler<KickMemberCommand, KickMemberOutcome>
{
    public async Task<KickMemberOutcome> HandleAsync(KickMemberCommand command, CancellationToken cancellationToken = default)
    {
        if (!await guilds.HasPermissionAsync(command.GuildId, command.PlayerId, GuildPermission.KickMember, cancellationToken))
        {
            return KickMemberOutcome.Forbidden;
        }

        var membership = await guilds.GetMembershipAsync(command.GuildId, command.CharacterId, cancellationToken);
        if (membership is null)
        {
            return KickMemberOutcome.MembershipNotFound;
        }

        var rank = await guilds.GetRankAsync(command.GuildId, membership.GuildRankId, cancellationToken);
        if (rank is not null && rank.IsLeader)
        {
            return KickMemberOutcome.CannotKickLeader;
        }

        guilds.RemoveMembership(membership);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return KickMemberOutcome.Kicked;
    }
}
