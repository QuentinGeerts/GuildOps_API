using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

public sealed record PlayerInvitationDto(
    Guid GuildId,
    string GuildName,
    string Server,
    Guid CharacterId,
    string CharacterName,
    string? Message,
    DateTimeOffset CreatedAt)
{
    public static PlayerInvitationDto From(GuildInvitation invitation)
        => new(invitation.GuildId,
               invitation.Guild!.Name,
               invitation.Guild.Server,
               invitation.CharacterId,
               invitation.Character!.Name,
               invitation.Message,
               invitation.CreatedAt);
}
