namespace GuildOps.Application.Guilds;

public sealed record SearchGuildsQuery(Guid? GameId, string? Server, string? Name);
