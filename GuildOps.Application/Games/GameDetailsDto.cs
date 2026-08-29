using GuildOps.Domain.Games;

namespace GuildOps.Application.Games;

public sealed record GameDetailsDto(Guid Id, string Name, int MaxLevel, IReadOnlyList<CharacterClassDto> Classes)
{
    public static GameDetailsDto From(Game game)
        => new(game.Id, game.Name, game.MaxLevel, [.. game.Classes.Select(CharacterClassDto.From)]);
}
