using GuildOps.API.Extensions;
using GuildOps.Application.Abstractions;
using GuildOps.Application.Players;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuildOps.API.Controllers;

[ApiController]
[Route("api/characters")]
[Authorize]
public sealed class CharactersController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CharacterDto>> Create(
        CreateCharacterRequest request,
        [FromServices] ICommandHandler<CreateCharacterCommand, CreateCharacterResult> createCharacter,
        CancellationToken cancellationToken)
    {
        var command = CreateCharacterCommand.From(User.GetPlayerId(), request);
        var result = await createCharacter.HandleAsync(command, cancellationToken);

        return result.Outcome switch
        {
            CreateCharacterOutcome.PlayerNotFound => Problem(
                detail: "Le compte associe a ce jeton n'existe plus.",
                statusCode: StatusCodes.Status401Unauthorized),

            CreateCharacterOutcome.GameNotFound => Problem(
                detail: "Ce jeu n'existe pas.",
                statusCode: StatusCodes.Status404NotFound),

            CreateCharacterOutcome.ClassNotInGame => Problem(
                detail: "Cette classe n'appartient pas au jeu choisi.",
                statusCode: StatusCodes.Status400BadRequest),

            CreateCharacterOutcome.LevelOutOfRange => Problem(
                detail: "Le niveau depasse le niveau maximum de ce jeu.",
                statusCode: StatusCodes.Status400BadRequest),

            CreateCharacterOutcome.NameTakenOnServer => Problem(
                detail: "Ce nom est deja pris sur ce serveur.",
                statusCode: StatusCodes.Status409Conflict),

            _ => Created($"/api/characters/{result.Character!.Id}", result.Character)
        };
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CharacterDto>> GetById(
        Guid id,
        [FromServices] IQueryHandler<GetCharacterByIdQuery, CharacterDto?> getCharacter,
        CancellationToken cancellationToken)
    {
        var character = await getCharacter.HandleAsync(new GetCharacterByIdQuery(id), cancellationToken);
        return character is null ? NotFound() : Ok(character);
    }

}
