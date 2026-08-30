using GuildOps.API.Extensions;
using GuildOps.Application.Abstractions;
using GuildOps.Application.Players;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuildOps.API.Controllers;

[ApiController]
[Route("api/players")]
public sealed class PlayersController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisteredPlayerDto>> Register(
        RegisterPlayerCommand command,
        [FromServices] ICommandHandler<RegisterPlayerCommand, RegisterPlayerResult> registerPlayer,
        CancellationToken cancellationToken)
    {
        var result = await registerPlayer.HandleAsync(command, cancellationToken);

        return result.Outcome switch
        {
            RegisterPlayerOutcome.AccountNameTaken => Problem(
                detail: "Ce nom de compte est deja utilise.",
                statusCode: StatusCodes.Status409Conflict),

            RegisterPlayerOutcome.EmailTaken => Problem(
                detail: "Cette adresse e-mail est deja utilisee.",
                statusCode: StatusCodes.Status409Conflict),
            _ => Created($"/api/players/{result.Player!.Id}", result.Player)
        };
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerDto>> GetMe(
        [FromServices] IQueryHandler<GetPlayerQuery, PlayerDto?> getPlayer,
        CancellationToken cancellationToken)
    {
        var player = await getPlayer.HandleAsync(new GetPlayerQuery(User.GetPlayerId()), cancellationToken);
        return player is null ? NotFound() : Ok(player);
    }

}
