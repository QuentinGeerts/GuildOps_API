using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Players;

internal sealed class DeleteCharacterCommandHandler(
    IPlayerRepository players,
    IGuildRepository guilds,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCharacterCommand, DeleteCharacterOutcome>
{
    public async Task<DeleteCharacterOutcome> HandleAsync(DeleteCharacterCommand command, CancellationToken cancellationToken = default)
    {
        var character = await players.GetCharacterForUpdateAsync(command.CharacterId, cancellationToken);

        if (character is null || character.PlayerId != command.PlayerId)
        {
            return DeleteCharacterOutcome.CharacterNotFound;
        }

        // pas de guilde orpheline : supprimer le chef emporte la guilde, ses grades et ses adhesions
        var ledGuild = await guilds.GetGuildLedByCharacterAsync(character.Id, cancellationToken);
        if (ledGuild is not null)
        {
            guilds.RemoveGuild(ledGuild);
        }

        players.RemoveCharacter(character);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DeleteCharacterOutcome.Deleted;
    }
}
