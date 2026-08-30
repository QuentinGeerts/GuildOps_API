namespace GuildOps.Application.Guilds;

public sealed record ApplyToGuildCommand(Guid PlayerId, Guid GuildId, Guid CharacterId, string? Message)
{
    public static ApplyToGuildCommand From(Guid playerId, Guid guildId, ApplyToGuildRequest request)
        => new(playerId, guildId, request.CharacterId, request.Message);
}
