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

    [HttpPost("{guildId:guid}/invitations")]
    public async Task<ActionResult<GuildInvitationDto>> Invite(
        Guid guildId,
        InviteCharacterRequest request,
        [FromServices] ICommandHandler<InviteCharacterCommand, InviteCharacterResult> invite,
        CancellationToken cancellationToken)
    {
        var result = await invite.HandleAsync(InviteCharacterCommand.From(User.GetPlayerId(), guildId, request), cancellationToken);

        return result.Outcome switch
        {
            InviteCharacterOutcome.Forbidden => Problem(
                detail: "Vous n'avez pas le droit d'inviter dans cette guilde.",
                statusCode: StatusCodes.Status403Forbidden),

            InviteCharacterOutcome.GuildNotFound => Problem(
                detail: "Cette guilde n'existe pas.",
                statusCode: StatusCodes.Status404NotFound),

            InviteCharacterOutcome.CharacterNotFound => Problem(
                detail: "Ce personnage n'existe pas.",
                statusCode: StatusCodes.Status404NotFound),

            InviteCharacterOutcome.DifferentGameOrServer => Problem(
                detail: "Ce personnage n'est pas sur le meme jeu ou le meme serveur que la guilde.",
                statusCode: StatusCodes.Status400BadRequest),

            InviteCharacterOutcome.CharacterAlreadyInGuild => Problem(
                detail: "Ce personnage appartient deja a une guilde.",
                statusCode: StatusCodes.Status409Conflict),

            InviteCharacterOutcome.AlreadyInvited => Problem(
                detail: "Une invitation est deja en cours pour ce personnage.",
                statusCode: StatusCodes.Status409Conflict),

            _ => Created($"/api/guilds/{guildId}/invitations", result.Invitation)
        };
    }

    [HttpGet("{guildId:guid}/invitations")]
    public async Task<ActionResult<IReadOnlyList<GuildInvitationDto>>> GetInvitations(
        Guid guildId,
        [FromServices] IQueryHandler<GetGuildInvitationsQuery, GuildInvitationsResult> getInvitations,
        CancellationToken cancellationToken)
    {
        var result = await getInvitations.HandleAsync(new GetGuildInvitationsQuery(User.GetPlayerId(), guildId), cancellationToken);

        return result.Outcome == GuildInvitationsOutcome.Forbidden
            ? Problem(detail: "Vous n'avez pas le droit d'inviter dans cette guilde.",
                      statusCode: StatusCodes.Status403Forbidden)
            : Ok(result.Invitations);
    }

    [HttpPost("{guildId:guid}/invitations/{characterId:guid}/accept")]
    public async Task<IActionResult> AcceptInvitation(
        Guid guildId,
        Guid characterId,
        [FromServices] ICommandHandler<AcceptGuildInvitationCommand, AcceptGuildInvitationOutcome> accept,
        CancellationToken cancellationToken)
    {
        var outcome = await accept.HandleAsync(new AcceptGuildInvitationCommand(User.GetPlayerId(), guildId, characterId), cancellationToken);

        return outcome switch
        {
            AcceptGuildInvitationOutcome.CharacterNotOwned => Problem(
                detail: "Ce personnage n'existe pas ou ne vous appartient pas.",
                statusCode: StatusCodes.Status404NotFound),

            AcceptGuildInvitationOutcome.InvitationNotFound => Problem(
                detail: "Aucune invitation en cours pour ce personnage.",
                statusCode: StatusCodes.Status404NotFound),

            AcceptGuildInvitationOutcome.CharacterAlreadyInGuild => Problem(
                detail: "Ce personnage appartient deja a une guilde.",
                statusCode: StatusCodes.Status409Conflict),

            AcceptGuildInvitationOutcome.NoDefaultRank => Problem(
                detail: "Cette guilde n'a pas de grade par defaut.",
                statusCode: StatusCodes.Status409Conflict),

            _ => NoContent()
        };
    }

    [HttpDelete("{guildId:guid}/invitations/{characterId:guid}")]
    public async Task<IActionResult> DeclineInvitation(
        Guid guildId,
        Guid characterId,
        [FromServices] ICommandHandler<DeclineGuildInvitationCommand, DeclineGuildInvitationOutcome> decline,
        CancellationToken cancellationToken)
    {
        var outcome = await decline.HandleAsync(new DeclineGuildInvitationCommand(User.GetPlayerId(), guildId, characterId), cancellationToken);

        return outcome switch
        {
            DeclineGuildInvitationOutcome.Forbidden => Problem(
                detail: "Cette invitation ne vous concerne pas.",
                statusCode: StatusCodes.Status403Forbidden),

            DeclineGuildInvitationOutcome.InvitationNotFound => Problem(
                detail: "Aucune invitation en cours pour ce personnage.",
                statusCode: StatusCodes.Status404NotFound),

            _ => NoContent()
        };
    }

    [HttpPut("{guildId:guid}")]
    public async Task<IActionResult> EditProfile(
        Guid guildId,
        EditGuildProfileRequest request,
        [FromServices] ICommandHandler<EditGuildProfileCommand, EditGuildProfileOutcome> edit,
        CancellationToken cancellationToken)
    {
        var outcome = await edit.HandleAsync(EditGuildProfileCommand.From(User.GetPlayerId(), guildId, request), cancellationToken);

        return outcome switch
        {
            EditGuildProfileOutcome.Forbidden => Problem(
                detail: "Vous n'avez pas le droit de modifier le profil de cette guilde.",
                statusCode: StatusCodes.Status403Forbidden),

            EditGuildProfileOutcome.GuildNotFound => Problem(
                detail: "Cette guilde n'existe pas.",
                statusCode: StatusCodes.Status404NotFound),

            EditGuildProfileOutcome.NameTakenOnServer => Problem(
                detail: "Une guilde porte deja ce nom sur ce serveur.",
                statusCode: StatusCodes.Status409Conflict),

            _ => NoContent()
        };
    }

    [HttpPut("{guildId:guid}/members/{characterId:guid}/rank")]
    public async Task<IActionResult> AssignRank(
        Guid guildId,
        Guid characterId,
        AssignMemberRankRequest request,
        [FromServices] ICommandHandler<AssignMemberRankCommand, AssignMemberRankOutcome> assign,
        CancellationToken cancellationToken)
    {
        var outcome = await assign.HandleAsync(AssignMemberRankCommand.From(User.GetPlayerId(), guildId, characterId, request), cancellationToken);

        return outcome switch
        {
            AssignMemberRankOutcome.Forbidden => Problem(
                detail: "Vous n'avez pas le droit d'attribuer des grades dans cette guilde.",
                statusCode: StatusCodes.Status403Forbidden),

            AssignMemberRankOutcome.RankNotInGuild => Problem(
                detail: "Ce grade n'appartient pas a cette guilde.",
                statusCode: StatusCodes.Status400BadRequest),

            AssignMemberRankOutcome.CannotAssignLeaderRank => Problem(
                detail: "Le grade de chef ne s'attribue pas ainsi : il faut transferer la direction.",
                statusCode: StatusCodes.Status400BadRequest),

            AssignMemberRankOutcome.CannotDemoteLeader => Problem(
                detail: "Le chef de guilde ne peut pas changer de grade : il faut transferer la direction.",
                statusCode: StatusCodes.Status409Conflict),

            AssignMemberRankOutcome.MembershipNotFound => Problem(
                detail: "Ce personnage n'est pas membre de cette guilde.",
                statusCode: StatusCodes.Status404NotFound),

            _ => NoContent()
        };
    }

    [HttpPut("{guildId:guid}/members/{characterId:guid}/note")]
    public async Task<IActionResult> SetMemberNote(
        Guid guildId,
        Guid characterId,
        SetMemberNoteRequest request,
        [FromServices] ICommandHandler<SetMemberNoteCommand, SetMemberNoteOutcome> setNote,
        CancellationToken cancellationToken)
    {
        var outcome = await setNote.HandleAsync(SetMemberNoteCommand.From(User.GetPlayerId(), guildId, characterId, request), cancellationToken);

        return outcome switch
        {
            SetMemberNoteOutcome.Forbidden => Problem(
                detail: "Vous n'avez pas le droit d'annoter les membres de cette guilde.",
                statusCode: StatusCodes.Status403Forbidden),

            SetMemberNoteOutcome.MembershipNotFound => Problem(
                detail: "Ce personnage n'est pas membre de cette guilde.",
                statusCode: StatusCodes.Status404NotFound),

            _ => NoContent()
        };
    }

    [HttpDelete("{guildId:guid}/members/{characterId:guid}")]
    public async Task<IActionResult> KickMember(
        Guid guildId,
        Guid characterId,
        [FromServices] ICommandHandler<KickMemberCommand, KickMemberOutcome> kick,
        CancellationToken cancellationToken)
    {
        var outcome = await kick.HandleAsync(new KickMemberCommand(User.GetPlayerId(), guildId, characterId), cancellationToken);

        return outcome switch
        {
            KickMemberOutcome.Forbidden => Problem(
                detail: "Vous n'avez pas le droit d'exclure des membres de cette guilde.",
                statusCode: StatusCodes.Status403Forbidden),

            KickMemberOutcome.MembershipNotFound => Problem(
                detail: "Ce personnage n'est pas membre de cette guilde.",
                statusCode: StatusCodes.Status404NotFound),

            KickMemberOutcome.CannotKickLeader => Problem(
                detail: "Le chef de guilde ne peut pas etre exclu.",
                statusCode: StatusCodes.Status409Conflict),

            _ => NoContent()
        };
    }
}
