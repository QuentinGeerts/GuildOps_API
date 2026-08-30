using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Players;

internal sealed class GetPlayerQueryHandler(IPlayerRepository players)
    : IQueryHandler<GetPlayerQuery, PlayerDto?>
{
    public async Task<PlayerDto?> HandleAsync(GetPlayerQuery query, CancellationToken cancellationToken = default)
    {
        var player = await players.GetWithCharactersAsync(query.Id, cancellationToken);
        return player is null ? null : PlayerDto.From(player);
    }
}
