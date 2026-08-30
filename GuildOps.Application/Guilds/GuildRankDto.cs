using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

public sealed record GuildRankDto(
    Guid Id,
    string Name,
    int SortOrder,
    bool IsLeader,
    bool IsDefault,
    IReadOnlyList<GuildPermission> Permissions)
{
    public static GuildRankDto From(GuildRank rank)
        => new(rank.Id, rank.Name, rank.SortOrder, rank.IsLeader, rank.IsDefault, rank.Permissions);
}
