namespace GuildOps.Application.Guilds;

public sealed record RejectGuildApplicationCommand(Guid PlayerId, Guid GuildId, Guid CharacterId);

public enum RejectGuildApplicationOutcome
{
    Rejected = 1,
    Forbidden = 2,
    ApplicationNotFound = 3
}
