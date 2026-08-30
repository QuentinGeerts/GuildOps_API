using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal sealed class AcceptGuildInvitationCommandHandler(
    IPlayerRepository players,
    IGuildRepository guilds,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AcceptGuildInvitationCommand, AcceptGuildInvitationOutcome>
{
    private const string MembershipIndex = "IX_GuildMemberships_CharacterId";

    public async Task<AcceptGuildInvitationOutcome> HandleAsync(AcceptGuildInvitationCommand command, CancellationToken cancellationToken = default)
    {
        var character = await players.GetCharacterAsync(command.CharacterId, cancellationToken);

        if (character is null || character.PlayerId != command.PlayerId)
        {
            return AcceptGuildInvitationOutcome.CharacterNotOwned;
        }

        var invitation = await guilds.GetInvitationAsync(command.GuildId, command.CharacterId, cancellationToken);
        if (invitation is null)
        {
            return AcceptGuildInvitationOutcome.InvitationNotFound;
        }

        if (await guilds.CharacterHasMembershipAsync(command.CharacterId, cancellationToken))
        {
            return AcceptGuildInvitationOutcome.CharacterAlreadyInGuild;
        }

        Guid? defaultRankId = await guilds.GetDefaultRankIdAsync(command.GuildId, cancellationToken);
        if (defaultRankId is null)
        {
            return AcceptGuildInvitationOutcome.NoDefaultRank;
        }

        guilds.AddMembership(new GuildMembership(command.GuildId, command.CharacterId, defaultRankId.Value));
        guilds.RemoveInvitation(invitation);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException exception) when (exception.ConstraintName == MembershipIndex)
        {
            return AcceptGuildInvitationOutcome.CharacterAlreadyInGuild;
        }

        return AcceptGuildInvitationOutcome.Accepted;
    }
}
