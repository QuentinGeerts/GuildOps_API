using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal sealed class EditGuildProfileCommandHandler(IGuildRepository guilds, IUnitOfWork unitOfWork)
    : ICommandHandler<EditGuildProfileCommand, EditGuildProfileOutcome>
{
    private const string ServerNameIndex = "IX_Guilds_Server_Name";

    public async Task<EditGuildProfileOutcome> HandleAsync(EditGuildProfileCommand command, CancellationToken cancellationToken = default)
    {
        if (!await guilds.HasPermissionAsync(command.GuildId, command.PlayerId, GuildPermission.EditGuildProfile, cancellationToken))
        {
            return EditGuildProfileOutcome.Forbidden;
        }

        var guild = await guilds.GetForUpdateAsync(command.GuildId, cancellationToken);
        if (guild is null)
        {
            return EditGuildProfileOutcome.GuildNotFound;
        }

        guild.Name = command.Name;
        guild.Description = command.Description;
        guild.ChatUrl = command.ChatUrl;

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException exception) when (exception.ConstraintName == ServerNameIndex)
        {
            return EditGuildProfileOutcome.NameTakenOnServer;
        }

        return EditGuildProfileOutcome.Updated;
    }
}
