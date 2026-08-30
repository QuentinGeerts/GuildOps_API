using GuildOps.Domain.Games;

namespace GuildOps.Application.Games;

public sealed record GameDetailsDto(Guid Id, string Name, int MaxLevel, IReadOnlyList<CharacterClassDto> Classes, IReadOnlyList<GameRoleDto> Roles)
{
    public static GameDetailsDto From(Game game)
        => new(game.Id, game.Name, game.MaxLevel, [.. game.Classes.Select(CharacterClassDto.From)],
               [.. game.Roles.OrderBy(role => role.SortOrder).Select(GameRoleDto.From)]);
}
