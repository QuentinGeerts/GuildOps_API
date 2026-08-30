using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal sealed class GetGuildApplicationsQueryHandler(IGuildRepository guilds)
    : IQueryHandler<GetGuildApplicationsQuery, GuildApplicationsResult>
{
    public async Task<GuildApplicationsResult> HandleAsync(GetGuildApplicationsQuery query, CancellationToken cancellationToken = default)
    {
        if (!await guilds.HasPermissionAsync(query.GuildId, query.PlayerId, GuildPermission.ReviewApplications, cancellationToken))
        {
            return GuildApplicationsResult.Forbidden;
        }

        var applications = await guilds.GetApplicationsAsync(query.GuildId, cancellationToken);

        return GuildApplicationsResult.Retrieved([.. applications.Select(GuildApplicationDto.From)]);
    }
}
