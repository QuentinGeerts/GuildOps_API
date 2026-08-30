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
    public async Task<ActionResult<AccessToken>> Login(
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

            _ => Ok(result.Token)
        };
    }
}
