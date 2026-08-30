namespace GuildOps.Application.Guilds;

public enum ApplyToGuildOutcome
{
    Created = 1,
    CharacterNotFound = 2,
    GuildNotFound = 3,
    DifferentGameOrServer = 4,
    CharacterAlreadyInGuild = 5,
    AlreadyApplied = 6
}

public sealed record ApplyToGuildResult(ApplyToGuildOutcome Outcome, GuildApplicationDto? Application)
{
    public static ApplyToGuildResult Created(GuildApplicationDto application)
        => new(ApplyToGuildOutcome.Created, application);

    public static ApplyToGuildResult Rejected(ApplyToGuildOutcome outcome) => new(outcome, null);
}
