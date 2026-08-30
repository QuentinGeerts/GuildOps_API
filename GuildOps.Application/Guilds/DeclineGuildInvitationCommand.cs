namespace GuildOps.Application.Guilds;

public sealed record DeclineGuildInvitationCommand(Guid PlayerId, Guid GuildId, Guid CharacterId);

public enum DeclineGuildInvitationOutcome
{
    Declined = 1,
    InvitationNotFound = 2,
    Forbidden = 3
}
