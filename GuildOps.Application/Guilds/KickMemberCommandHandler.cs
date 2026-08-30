using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal sealed class KickMemberCommandHandler(
    IPlayerRepository players,
    IGuildRepository guilds,
    IUnitOfWork unitOfWork)
    : ICommandHandler<KickMemberCommand, KickMemberOutcome>
{
    public async Task<KickMemberOutcome> HandleAsync(KickMemberCommand command, CancellationToken cancellationToken = default)
    {
        var membership = await guilds.GetMembershipAsync(command.GuildId, command.CharacterId, cancellationToken);
        if (membership is null)
        {
            return KickMemberOutcome.MembershipNotFound;
        }

        // deux acteurs legitimes : le membre qui part de lui-meme, ou un grade qui exclut
        var character = await players.GetCharacterAsync(command.CharacterId, cancellationToken);
        bool leavesOwnGuild = character is not null && character.PlayerId == command.PlayerId;

        if (!leavesOwnGuild
            && !await guilds.HasPermissionAsync(command.GuildId, command.PlayerId, GuildPermission.KickMember, cancellationToken))
        {
            return KickMemberOutcome.Forbidden;
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
