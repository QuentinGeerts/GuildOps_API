using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Guilds;

internal sealed class SearchGuildsQueryHandler(IGuildRepository guilds)
    : IQueryHandler<SearchGuildsQuery, IReadOnlyList<GuildSummaryDto>>
{
    public Task<IReadOnlyList<GuildSummaryDto>> HandleAsync(SearchGuildsQuery query, CancellationToken cancellationToken = default)
        => guilds.SearchAsync(query.GameId, query.Server, query.Name, cancellationToken);
}
