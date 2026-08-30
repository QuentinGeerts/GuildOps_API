using GuildOps.Domain.Guilds;
using GuildOps.Domain.Players;

namespace GuildOps.Application.Guilds;

public sealed record GuildInvitationDto(
    Guid GuildId,
    Guid CharacterId,
    string CharacterName,
    int Level,
    string? Message,
    DateTimeOffset CreatedAt)
{
    public static GuildInvitationDto From(GuildInvitation invitation)
        => From(invitation, invitation.Character!);

    public static GuildInvitationDto From(GuildInvitation invitation, Character character)
        => new(invitation.GuildId, invitation.CharacterId, character.Name, character.Level,
               invitation.Message, invitation.CreatedAt);
}
