using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Guilds;

internal sealed class TransferLeadershipCommandHandler(IGuildRepository guilds, IUnitOfWork unitOfWork)
    : ICommandHandler<TransferLeadershipCommand, TransferLeadershipOutcome>
{
    public async Task<TransferLeadershipOutcome> HandleAsync(TransferLeadershipCommand command, CancellationToken cancellationToken = default)
    {
        var leadership = await guilds.GetLeaderMembershipAsync(command.GuildId, cancellationToken);

        if (leadership is null || leadership.Character!.PlayerId != command.PlayerId)
        {
            return TransferLeadershipOutcome.NotLeader;
        }

        if (leadership.CharacterId == command.CharacterId)
        {
            return TransferLeadershipOutcome.AlreadyLeader;
        }

        var successor = await guilds.GetMembershipAsync(command.GuildId, command.CharacterId, cancellationToken);
        if (successor is null)
        {
            return TransferLeadershipOutcome.MembershipNotFound;
        }

        // echange des grades : la direction change de titulaire sans jamais rester vacante
        (leadership.GuildRankId, successor.GuildRankId) = (successor.GuildRankId, leadership.GuildRankId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TransferLeadershipOutcome.Transferred;
    }
}
