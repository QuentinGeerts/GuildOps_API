using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal sealed class RejectGuildApplicationCommandHandler(IGuildRepository guilds, IUnitOfWork unitOfWork)
    : ICommandHandler<RejectGuildApplicationCommand, RejectGuildApplicationOutcome>
{
    public async Task<RejectGuildApplicationOutcome> HandleAsync(RejectGuildApplicationCommand command, CancellationToken cancellationToken = default)
    {
        if (!await guilds.HasPermissionAsync(command.GuildId, command.PlayerId, GuildPermission.ReviewApplications, cancellationToken))
        {
            return RejectGuildApplicationOutcome.Forbidden;
        }

        var application = await guilds.GetApplicationAsync(command.GuildId, command.CharacterId, cancellationToken);
        if (application is null)
        {
            return RejectGuildApplicationOutcome.ApplicationNotFound;
        }

        guilds.RemoveApplication(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return RejectGuildApplicationOutcome.Rejected;
    }
}
