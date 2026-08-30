using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

public sealed record GuildDto(
    Guid Id,
    Guid GameId,
    string Name,
    string Server,
    string? Description,
    string? ChatUrl,
    DateTimeOffset CreatedAt,
    IReadOnlyList<GuildRankDto> Ranks)
{
    public static GuildDto From(Guild guild)
        => new(guild.Id, guild.GameId, guild.Name, guild.Server, guild.Description, guild.ChatUrl, guild.CreatedAt,
               [.. guild.Ranks.OrderBy(rank => rank.SortOrder).Select(GuildRankDto.From)]);
}
