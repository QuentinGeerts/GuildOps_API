using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal sealed class SetMemberNoteCommandHandler(IGuildRepository guilds, IUnitOfWork unitOfWork)
    : ICommandHandler<SetMemberNoteCommand, SetMemberNoteOutcome>
{
    public async Task<SetMemberNoteOutcome> HandleAsync(SetMemberNoteCommand command, CancellationToken cancellationToken = default)
    {
        if (!await guilds.HasPermissionAsync(command.GuildId, command.PlayerId, GuildPermission.WriteMemberNote, cancellationToken))
        {
            return SetMemberNoteOutcome.Forbidden;
        }

        var membership = await guilds.GetMembershipAsync(command.GuildId, command.CharacterId, cancellationToken);
        if (membership is null)
        {
            return SetMemberNoteOutcome.MembershipNotFound;
        }

        membership.Note = command.Note;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return SetMemberNoteOutcome.Updated;
    }
}
