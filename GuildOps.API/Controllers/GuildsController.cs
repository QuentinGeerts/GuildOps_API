using GuildOps.API.Extensions;
using GuildOps.Application.Abstractions;
using GuildOps.Application.Guilds;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuildOps.API.Controllers;

[ApiController]
[Route("api/guilds")]
[Authorize]
public sealed class GuildsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GuildDto>> Create(
        CreateGuildRequest request,
        [FromServices] ICommandHandler<CreateGuildCommand, CreateGuildResult> createGuild,
        CancellationToken cancellationToken)
    {
        var command = CreateGuildCommand.From(User.GetPlayerId(), request);
        var result = await createGuild.HandleAsync(command, cancellationToken);

        return result.Outcome switch
        {
            CreateGuildOutcome.CharacterNotFound => Problem(
                detail: "Ce personnage n'existe pas ou ne vous appartient pas.",
                statusCode: StatusCodes.Status404NotFound),

            CreateGuildOutcome.CharacterAlreadyInGuild => Problem(
                detail: "Ce personnage appartient deja a une guilde.",
                statusCode: StatusCodes.Status409Conflict),

            CreateGuildOutcome.NameTakenOnServer => Problem(
                detail: "Une guilde porte deja ce nom sur ce serveur.",
                statusCode: StatusCodes.Status409Conflict),

            _ => Created($"/api/guilds/{result.Guild!.Id}", result.Guild)
        };
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GuildDetailsDto>> GetById(
        Guid id,
        [FromServices] IQueryHandler<GetGuildByIdQuery, GuildDetailsDto?> getGuild,
        CancellationToken cancellationToken)
    {
        var guild = await getGuild.HandleAsync(new GetGuildByIdQuery(id), cancellationToken);
        return guild is null ? NotFound() : Ok(guild);
    }

}
