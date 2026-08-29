using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Games;

internal sealed class GetGameByIdQueryHandler(IGameRepository repository)
    : IQueryHandler<GetGameByIdQuery, GameDetailsDto?>
{
    public async Task<GameDetailsDto?> HandleAsync(GetGameByIdQuery query, CancellationToken cancellationToken = default)
    {
        var game = await repository.GetWithClassesAsync(query.Id, cancellationToken);
        return game is null ? null : GameDetailsDto.From(game);
    }
}
