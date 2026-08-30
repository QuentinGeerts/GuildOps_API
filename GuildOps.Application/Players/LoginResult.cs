namespace GuildOps.Application.Players;

public enum LoginOutcome
{
    Succeeded = 1,
    InvalidCredentials = 2
}

public sealed record LoginResult(LoginOutcome Outcome, AuthTokensDto? Tokens)
{
    public static LoginResult Succeeded(AuthTokensDto tokens) => new(LoginOutcome.Succeeded, tokens);

    public static readonly LoginResult InvalidCredentials = new(LoginOutcome.InvalidCredentials, null);
}
