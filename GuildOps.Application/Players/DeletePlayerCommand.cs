namespace GuildOps.Application.Players;

public sealed record DeletePlayerCommand(Guid PlayerId);

public enum DeletePlayerOutcome
{
    Deleted = 1,
    PlayerNotFound = 2
}
