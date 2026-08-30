using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Guilds;

internal sealed class GetPlayerInvitationsQueryHandler(IGuildRepository guilds)
    : IQueryHandler<GetPlayerInvitationsQuery, IReadOnlyList<PlayerInvitationDto>>
{
    public async Task<IReadOnlyList<PlayerInvitationDto>> HandleAsync(GetPlayerInvitationsQuery query, CancellationToken cancellationToken = default)
    {
        var invitations = await guilds.GetInvitationsForPlayerAsync(query.PlayerId, cancellationToken);

        return [.. invitations.Select(PlayerInvitationDto.From)];
    }
}
