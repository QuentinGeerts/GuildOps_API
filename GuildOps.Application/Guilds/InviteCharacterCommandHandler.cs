using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal sealed class InviteCharacterCommandHandler(
    IPlayerRepository players,
    IGuildRepository guilds,
    IUnitOfWork unitOfWork)
    : ICommandHandler<InviteCharacterCommand, InviteCharacterResult>
{
    private const string InvitationIndex = "IX_GuildInvitations_GuildId_CharacterId";

    public async Task<InviteCharacterResult> HandleAsync(InviteCharacterCommand command, CancellationToken cancellationToken = default)
    {
        if (!await guilds.HasPermissionAsync(command.GuildId, command.PlayerId, GuildPermission.InviteMember, cancellationToken))
        {
            return InviteCharacterResult.Rejected(InviteCharacterOutcome.Forbidden);
        }

        var guild = await guilds.GetAsync(command.GuildId, cancellationToken);
        if (guild is null)
        {
            return InviteCharacterResult.Rejected(InviteCharacterOutcome.GuildNotFound);
        }

        var character = await players.GetCharacterAsync(command.CharacterId, cancellationToken);
        if (character is null)
        {
            return InviteCharacterResult.Rejected(InviteCharacterOutcome.CharacterNotFound);
        }

        if (guild.GameId != character.GameId || guild.Server != character.Server)
        {
            return InviteCharacterResult.Rejected(InviteCharacterOutcome.DifferentGameOrServer);
        }

        if (await guilds.CharacterHasMembershipAsync(character.Id, cancellationToken))
        {
            return InviteCharacterResult.Rejected(InviteCharacterOutcome.CharacterAlreadyInGuild);
        }

        if (await guilds.InvitationExistsAsync(guild.Id, character.Id, cancellationToken))
        {
            return InviteCharacterResult.Rejected(InviteCharacterOutcome.AlreadyInvited);
        }

        var invitation = new GuildInvitation(guild.Id, character.Id, command.Message);
        guilds.AddInvitation(invitation);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException exception) when (exception.ConstraintName == InvitationIndex)
        {
            return InviteCharacterResult.Rejected(InviteCharacterOutcome.AlreadyInvited);
        }

        return InviteCharacterResult.Created(GuildInvitationDto.From(invitation, character));
    }
}
