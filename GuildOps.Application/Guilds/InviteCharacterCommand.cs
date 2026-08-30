namespace GuildOps.Application.Guilds;

public sealed record InviteCharacterCommand(Guid PlayerId, Guid GuildId, Guid CharacterId, string? Message)
{
    public static InviteCharacterCommand From(Guid playerId, Guid guildId, InviteCharacterRequest request)
        => new(playerId, guildId, request.CharacterId, request.Message);
}
