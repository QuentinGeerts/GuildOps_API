using System.ComponentModel.DataAnnotations;

namespace GuildOps.Application.Players;

public sealed record RefreshTokensCommand([Required, StringLength(256)] string RefreshToken);

public enum RefreshTokensOutcome
{
    Refreshed = 1,
    InvalidToken = 2
}

public sealed record RefreshTokensResult(RefreshTokensOutcome Outcome, AuthTokensDto? Tokens)
{
    public static RefreshTokensResult Refreshed(AuthTokensDto tokens) => new(RefreshTokensOutcome.Refreshed, tokens);

    public static readonly RefreshTokensResult InvalidToken = new(RefreshTokensOutcome.InvalidToken, null);
}
