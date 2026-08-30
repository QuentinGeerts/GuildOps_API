namespace GuildOps.Application.Guilds;

public sealed record AcceptGuildApplicationCommand(Guid PlayerId, Guid GuildId, Guid CharacterId);

public enum AcceptGuildApplicationOutcome
{
    Accepted = 1,
    Forbidden = 2,
    ApplicationNotFound = 3,
    CharacterAlreadyInGuild = 4,
    NoDefaultRank = 5
}
