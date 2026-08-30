using GuildOps.Application.Abstractions;
using GuildOps.Domain.Players;

namespace GuildOps.Application.Players;

internal sealed class SetCharacterRolesCommandHandler(
    IPlayerRepository players,
    IGameRepository games,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SetCharacterRolesCommand, SetCharacterRolesOutcome>
{
    public async Task<SetCharacterRolesOutcome> HandleAsync(SetCharacterRolesCommand command, CancellationToken cancellationToken = default)
    {
        var character = await players.GetCharacterForUpdateAsync(command.CharacterId, cancellationToken);

        if (character is null || character.PlayerId != command.PlayerId)
        {
            return SetCharacterRolesOutcome.CharacterNotFound;
        }

        var game = await games.GetWithClassesAndRolesAsync(character.GameId, cancellationToken);
        if (game is null)
        {
            return SetCharacterRolesOutcome.CharacterNotFound;
        }

        List<Guid> requested = [.. command.RoleIds.Distinct()];

        if (requested.Any(roleId => game.Roles.All(role => role.Id != roleId)))
        {
            return SetCharacterRolesOutcome.RoleNotInGame;
        }

        foreach (var obsolete in character.Roles.Where(assignment => !requested.Contains(assignment.GameRoleId)).ToList())
        {
            players.RemoveCharacterRole(obsolete);
        }

        foreach (Guid roleId in requested.Where(roleId => character.Roles.All(assignment => assignment.GameRoleId != roleId)))
        {
            players.AddCharacterRole(new CharacterGameRole(character.Id, roleId));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return SetCharacterRolesOutcome.Updated;
    }
}
