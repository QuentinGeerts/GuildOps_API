using System.ComponentModel.DataAnnotations;

namespace GuildOps.Application.Players;

public sealed record SetCharacterRolesRequest([Required] IReadOnlyList<Guid> RoleIds);

public sealed record SetCharacterRolesCommand(Guid PlayerId, Guid CharacterId, IReadOnlyList<Guid> RoleIds)
{
    public static SetCharacterRolesCommand From(Guid playerId, Guid characterId, SetCharacterRolesRequest request)
        => new(playerId, characterId, request.RoleIds);
}

public enum SetCharacterRolesOutcome
{
    Updated = 1,
    CharacterNotFound = 2,
    RoleNotInGame = 3
}
