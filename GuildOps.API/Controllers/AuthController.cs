using GuildOps.Application.Abstractions;
using GuildOps.Application.Players;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuildOps.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokensDto>> Login(
        LoginCommand command,
        [FromServices] ICommandHandler<LoginCommand, LoginResult> login,
        CancellationToken cancellationToken)
    {
        var result = await login.HandleAsync(command, cancellationToken);

        return result.Outcome switch
        {
            LoginOutcome.InvalidCredentials => Problem(
                detail: "Adresse e-mail ou mot de passe incorrect.",
                statusCode: StatusCodes.Status401Unauthorized),

            _ => Ok(result.Tokens)
        };
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokensDto>> Refresh(
        RefreshTokensCommand command,
        [FromServices] ICommandHandler<RefreshTokensCommand, RefreshTokensResult> refresh,
        CancellationToken cancellationToken)
    {
        var result = await refresh.HandleAsync(command, cancellationToken);

        return result.Outcome == RefreshTokensOutcome.InvalidToken
            ? Problem(detail: "Jeton de rafraichissement invalide ou expire.",
                      statusCode: StatusCodes.Status401Unauthorized)
            : Ok(result.Tokens);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        LogoutCommand command,
        [FromServices] ICommandHandler<LogoutCommand> logout,
        CancellationToken cancellationToken)
    {
        await logout.HandleAsync(command, cancellationToken);

        return NoContent();
    }
}
