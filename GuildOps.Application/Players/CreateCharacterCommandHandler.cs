using GuildOps.Application.Abstractions;
using GuildOps.Domain.Players;

namespace GuildOps.Application.Players;

internal sealed class CreateCharacterCommandHandler(
    IPlayerRepository players,
    IGameRepository games,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCharacterCommand, CreateCharacterResult>
{
    private const string ServerNameIndex = "IX_Characters_Server_Name";

    public async Task<CreateCharacterResult> HandleAsync(CreateCharacterCommand command, CancellationToken cancellationToken = default)
    {
        if (!await players.ExistsAsync(command.PlayerId, cancellationToken))
        {
            return CreateCharacterResult.Rejected(CreateCharacterOutcome.PlayerNotFound);
        }

        var game = await games.GetWithClassesAsync(command.GameId, cancellationToken);
        if (game is null)
        {
            return CreateCharacterResult.Rejected(CreateCharacterOutcome.GameNotFound);
        }

        if (!game.Classes.Any(characterClass => characterClass.Id == command.CharacterClassId))
        {
            return CreateCharacterResult.Rejected(CreateCharacterOutcome.ClassNotInGame);
        }

        if (command.Level < 1 || command.Level > game.MaxLevel)
        {
            return CreateCharacterResult.Rejected(CreateCharacterOutcome.LevelOutOfRange);
        }

        if (await players.CharacterNameExistsAsync(command.Server, command.Name, cancellationToken))
        {
            return CreateCharacterResult.Rejected(CreateCharacterOutcome.NameTakenOnServer);
        }

        var character = new Character(
            command.PlayerId,
            command.GameId,
            command.CharacterClassId,
            command.Name,
            command.Server,
            command.Level);

        players.AddCharacter(character);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException exception) when (exception.ConstraintName == ServerNameIndex)
        {
            return CreateCharacterResult.Rejected(CreateCharacterOutcome.NameTakenOnServer);
        }

        return CreateCharacterResult.Created(CharacterDto.From(character));
    }
}
