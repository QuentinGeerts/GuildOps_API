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


    [HttpPost("{guildId:guid}/applications")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GuildApplicationDto>> Apply(
        Guid guildId,
        ApplyToGuildRequest request,
        [FromServices] ICommandHandler<ApplyToGuildCommand, ApplyToGuildResult> apply,
        CancellationToken cancellationToken)
    {
        var result = await apply.HandleAsync(ApplyToGuildCommand.From(User.GetPlayerId(), guildId, request), cancellationToken);

        return result.Outcome switch
        {
            ApplyToGuildOutcome.CharacterNotFound => Problem(
                detail: "Ce personnage n'existe pas ou ne vous appartient pas.",
                statusCode: StatusCodes.Status404NotFound),

            ApplyToGuildOutcome.GuildNotFound => Problem(
                detail: "Cette guilde n'existe pas.",
                statusCode: StatusCodes.Status404NotFound),

            ApplyToGuildOutcome.DifferentGameOrServer => Problem(
                detail: "Cette guilde n'est pas sur le meme jeu ou le meme serveur que votre personnage.",
                statusCode: StatusCodes.Status400BadRequest),

            ApplyToGuildOutcome.CharacterAlreadyInGuild => Problem(
                detail: "Ce personnage appartient deja a une guilde.",
                statusCode: StatusCodes.Status409Conflict),

            ApplyToGuildOutcome.AlreadyApplied => Problem(
                detail: "Une candidature est deja en cours pour ce personnage.",
                statusCode: StatusCodes.Status409Conflict),

            _ => Created($"/api/guilds/{guildId}/applications", result.Application)
        };
    }

    [HttpGet("{guildId:guid}/applications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<GuildApplicationDto>>> GetApplications(
        Guid guildId,
        [FromServices] IQueryHandler<GetGuildApplicationsQuery, GuildApplicationsResult> getApplications,
        CancellationToken cancellationToken)
    {
        var result = await getApplications.HandleAsync(new GetGuildApplicationsQuery(User.GetPlayerId(), guildId), cancellationToken);

        return result.Outcome == GuildApplicationsOutcome.Forbidden
            ? Problem(detail: "Vous n'avez pas le droit d'examiner les candidatures de cette guilde.",
                      statusCode: StatusCodes.Status403Forbidden)
            : Ok(result.Applications);
    }

    [HttpPost("{guildId:guid}/applications/{characterId:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AcceptApplication(
        Guid guildId,
        Guid characterId,
        [FromServices] ICommandHandler<AcceptGuildApplicationCommand, AcceptGuildApplicationOutcome> accept,
        CancellationToken cancellationToken)
    {
        var outcome = await accept.HandleAsync(new AcceptGuildApplicationCommand(User.GetPlayerId(), guildId, characterId), cancellationToken);

        return outcome switch
        {
            AcceptGuildApplicationOutcome.Forbidden => Problem(
                detail: "Vous n'avez pas le droit d'examiner les candidatures de cette guilde.",
                statusCode: StatusCodes.Status403Forbidden),

            AcceptGuildApplicationOutcome.ApplicationNotFound => Problem(
                detail: "Aucune candidature en cours pour ce personnage.",
                statusCode: StatusCodes.Status404NotFound),

            AcceptGuildApplicationOutcome.CharacterAlreadyInGuild => Problem(
                detail: "Ce personnage a rejoint une autre guilde entre-temps.",
                statusCode: StatusCodes.Status409Conflict),

            AcceptGuildApplicationOutcome.NoDefaultRank => Problem(
                detail: "Cette guilde n'a pas de grade par defaut.",
                statusCode: StatusCodes.Status409Conflict),

            _ => NoContent()
        };
    }

    [HttpDelete("{guildId:guid}/applications/{characterId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectApplication(
        Guid guildId,
        Guid characterId,
        [FromServices] ICommandHandler<RejectGuildApplicationCommand, RejectGuildApplicationOutcome> reject,
        CancellationToken cancellationToken)
    {
        var outcome = await reject.HandleAsync(new RejectGuildApplicationCommand(User.GetPlayerId(), guildId, characterId), cancellationToken);

        return outcome switch
        {
            RejectGuildApplicationOutcome.Forbidden => Problem(
                detail: "Vous n'avez pas le droit d'examiner les candidatures de cette guilde.",
                statusCode: StatusCodes.Status403Forbidden),

            RejectGuildApplicationOutcome.ApplicationNotFound => Problem(
                detail: "Aucune candidature en cours pour ce personnage.",
                statusCode: StatusCodes.Status404NotFound),

            _ => NoContent()
        };
    }
}
