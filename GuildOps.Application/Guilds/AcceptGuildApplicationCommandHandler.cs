using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal sealed class AcceptGuildApplicationCommandHandler(IGuildRepository guilds, IUnitOfWork unitOfWork)
    : ICommandHandler<AcceptGuildApplicationCommand, AcceptGuildApplicationOutcome>
{
    private const string MembershipIndex = "IX_GuildMemberships_CharacterId";

    public async Task<AcceptGuildApplicationOutcome> HandleAsync(AcceptGuildApplicationCommand command, CancellationToken cancellationToken = default)
    {
        if (!await guilds.HasPermissionAsync(command.GuildId, command.PlayerId, GuildPermission.ReviewApplications, cancellationToken))
        {
            return AcceptGuildApplicationOutcome.Forbidden;
        }

        var application = await guilds.GetApplicationAsync(command.GuildId, command.CharacterId, cancellationToken);
        if (application is null)
        {
            return AcceptGuildApplicationOutcome.ApplicationNotFound;
        }

        if (await guilds.CharacterHasMembershipAsync(command.CharacterId, cancellationToken))
        {
            return AcceptGuildApplicationOutcome.CharacterAlreadyInGuild;
        }

        Guid? defaultRankId = await guilds.GetDefaultRankIdAsync(command.GuildId, cancellationToken);
        if (defaultRankId is null)
        {
            return AcceptGuildApplicationOutcome.NoDefaultRank;
        }

        guilds.AddMembership(new GuildMembership(command.GuildId, command.CharacterId, defaultRankId.Value));
        guilds.RemoveApplication(application);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException exception) when (exception.ConstraintName == MembershipIndex)
        {
            return AcceptGuildApplicationOutcome.CharacterAlreadyInGuild;
        }

        return AcceptGuildApplicationOutcome.Accepted;
    }
}
