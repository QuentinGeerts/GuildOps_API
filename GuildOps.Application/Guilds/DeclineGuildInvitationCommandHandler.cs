using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal sealed class DeclineGuildInvitationCommandHandler(
    IPlayerRepository players,
    IGuildRepository guilds,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeclineGuildInvitationCommand, DeclineGuildInvitationOutcome>
{
    public async Task<DeclineGuildInvitationOutcome> HandleAsync(DeclineGuildInvitationCommand command, CancellationToken cancellationToken = default)
    {
        var invitation = await guilds.GetInvitationAsync(command.GuildId, command.CharacterId, cancellationToken);
        if (invitation is null)
        {
            return DeclineGuildInvitationOutcome.InvitationNotFound;
        }

        var character = await players.GetCharacterAsync(command.CharacterId, cancellationToken);
        bool ownsCharacter = character is not null && character.PlayerId == command.PlayerId;

        if (!ownsCharacter
            && !await guilds.HasPermissionAsync(command.GuildId, command.PlayerId, GuildPermission.InviteMember, cancellationToken))
        {
            return DeclineGuildInvitationOutcome.Forbidden;
        }

        guilds.RemoveInvitation(invitation);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DeclineGuildInvitationOutcome.Declined;
    }
}
