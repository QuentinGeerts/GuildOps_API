namespace GuildOps.Application.Guilds;

public enum CreateGuildOutcome
{
    Created = 1,
    CharacterNotFound = 2,
    CharacterAlreadyInGuild = 3,
    NameTakenOnServer = 4
}

public sealed record CreateGuildResult(CreateGuildOutcome Outcome, GuildDto? Guild)
{
    public static CreateGuildResult Created(GuildDto guild) => new(CreateGuildOutcome.Created, guild);

    public static CreateGuildResult Rejected(CreateGuildOutcome outcome) => new(outcome, null);
}
