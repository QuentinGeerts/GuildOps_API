namespace GuildOps.Application.Players;

public enum RegisterPlayerOutcome
{
    Created = 1,
    AccountNameTaken = 2,
    EmailTaken = 3
}

public sealed record RegisterPlayerResult(RegisterPlayerOutcome Outcome, RegisteredPlayerDto? Player)
{
    public static RegisterPlayerResult Created(RegisteredPlayerDto player)
        => new(RegisterPlayerOutcome.Created, player);

    public static RegisterPlayerResult Rejected(RegisterPlayerOutcome outcome)
        => new(outcome, null);
}
