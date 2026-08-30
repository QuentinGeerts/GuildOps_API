using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Guilds;

internal sealed class GetGuildByIdQueryHandler(IGuildRepository guilds)
    : IQueryHandler<GetGuildByIdQuery, GuildDetailsDto?>
{
    public async Task<GuildDetailsDto?> HandleAsync(GetGuildByIdQuery query, CancellationToken cancellationToken = default)
    {
        var guild = await guilds.GetWithMembersAsync(query.Id, cancellationToken);
        return guild is null ? null : GuildDetailsDto.From(guild);
    }
}
