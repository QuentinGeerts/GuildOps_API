using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

public sealed record GuildDetailsDto(
    Guid Id,
    Guid GameId,
    string Name,
    string Server,
    string? Description,
    string? ChatUrl,
    DateTimeOffset CreatedAt,
    IReadOnlyList<GuildRankDto> Ranks,
    IReadOnlyList<GuildMemberDto> Members)
{
    public static GuildDetailsDto From(Guild guild)
        => new(guild.Id, guild.GameId, guild.Name, guild.Server, guild.Description, guild.ChatUrl, guild.CreatedAt,
               [.. guild.Ranks.OrderBy(rank => rank.SortOrder).Select(GuildRankDto.From)],
               [.. guild.Memberships.OrderBy(membership => membership.Rank!.SortOrder)
                                    .ThenBy(membership => membership.Character!.Name)
                                    .Select(GuildMemberDto.From)]);
}
