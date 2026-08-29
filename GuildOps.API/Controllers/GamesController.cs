using GuildOps.Application.Abstractions;
using GuildOps.Application.Games;
using Microsoft.AspNetCore.Mvc;

namespace GuildOps.API.Controllers;

[ApiController]
[Route("api/games")]
public sealed class GamesController : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<GameDto>> GetAll(
        [FromServices] IQueryHandler<GetGamesQuery, IReadOnlyList<GameDto>> getGames,
        CancellationToken cancellationToken)
        => getGames.HandleAsync(new GetGamesQuery(), cancellationToken);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GameDetailsDto>> GetById(
        Guid id,
        [FromServices] IQueryHandler<GetGameByIdQuery, GameDetailsDto?> getGameById,
        CancellationToken cancellationToken)
    {
        var game = await getGameById.HandleAsync(new GetGameByIdQuery(id), cancellationToken);
        return game is null ? NotFound() : Ok(game);
    }
}
