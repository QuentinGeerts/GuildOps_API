namespace GuildOps.Application.Guilds;

public sealed record AcceptGuildInvitationCommand(Guid PlayerId, Guid GuildId, Guid CharacterId);

public enum AcceptGuildInvitationOutcome
{
    Accepted = 1,
    InvitationNotFound = 2,
    CharacterNotOwned = 3,
    CharacterAlreadyInGuild = 4,
    NoDefaultRank = 5
}
