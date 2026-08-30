using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal sealed class GetGuildInvitationsQueryHandler(IGuildRepository guilds)
    : IQueryHandler<GetGuildInvitationsQuery, GuildInvitationsResult>
{
    public async Task<GuildInvitationsResult> HandleAsync(GetGuildInvitationsQuery query, CancellationToken cancellationToken = default)
    {
        if (!await guilds.HasPermissionAsync(query.GuildId, query.PlayerId, GuildPermission.InviteMember, cancellationToken))
        {
            return GuildInvitationsResult.Forbidden;
        }

        var invitations = await guilds.GetInvitationsAsync(query.GuildId, cancellationToken);

        return GuildInvitationsResult.Retrieved([.. invitations.Select(GuildInvitationDto.From)]);
    }
}
