namespace GuildOps.Application.Guilds;

public sealed record CreateGuildCommand(
    Guid PlayerId,
    Guid CharacterId,
    string Name,
    string? Description,
    string? ChatUrl)
{
    public static CreateGuildCommand From(Guid playerId, CreateGuildRequest request)
        => new(playerId, request.CharacterId, request.Name, request.Description, request.ChatUrl);
}
