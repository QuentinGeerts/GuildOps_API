using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Games;

internal sealed class GetGamesQueryHandler(IGameRepository repository)
    : IQueryHandler<GetGamesQuery, IReadOnlyList<GameDto>>
{
    public async Task<IReadOnlyList<GameDto>> HandleAsync(GetGamesQuery query, CancellationToken cancellationToken = default)
    {
        var games = await repository.GetAllAsync(cancellationToken);
        return [.. games.Select(GameDto.From)];
    }
}
