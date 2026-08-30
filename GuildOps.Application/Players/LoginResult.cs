using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Players;

public enum LoginOutcome
{
    Succeeded = 1,
    InvalidCredentials = 2
}

public sealed record LoginResult(LoginOutcome Outcome, AccessToken? Token)
{
    public static LoginResult Succeeded(AccessToken token) => new(LoginOutcome.Succeeded, token);

    public static readonly LoginResult InvalidCredentials = new(LoginOutcome.InvalidCredentials, null);
}
