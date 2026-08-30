using GuildOps.Domain.Guilds;
using GuildOps.Domain.Players;

namespace GuildOps.Application.Guilds;

public sealed record GuildApplicationDto(
    Guid GuildId,
    Guid CharacterId,
    string CharacterName,
    int Level,
    string? Message,
    DateTimeOffset CreatedAt)
{
    public static GuildApplicationDto From(GuildApplication application)
        => From(application, application.Character!);

    public static GuildApplicationDto From(GuildApplication application, Character character)
        => new(application.GuildId, application.CharacterId, character.Name, character.Level,
               application.Message, application.CreatedAt);
}
