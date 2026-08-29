using GuildOps.Domain.Games;

namespace GuildOps.Application.Games;

public sealed record GameDto(Guid Id, string Name, int MaxLevel)
{
    public static GameDto From(Game game)
        => new(game.Id, game.Name, game.MaxLevel);
}
