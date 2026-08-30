namespace GuildOps.Application.Guilds;

public sealed record GuildSummaryDto(
    Guid Id,
    Guid GameId,
    string Name,
    string Server,
    string? Description,
    int MemberCount,
    DateTimeOffset CreatedAt);
