using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Players;

internal sealed class DeletePlayerCommandHandler(
    IPlayerRepository players,
    IGuildRepository guilds,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePlayerCommand, DeletePlayerOutcome>
{
    public async Task<DeletePlayerOutcome> HandleAsync(DeletePlayerCommand command, CancellationToken cancellationToken = default)
    {
        var player = await players.GetForUpdateAsync(command.PlayerId, cancellationToken);

        if (player is null)
        {
            return DeletePlayerOutcome.PlayerNotFound;
        }

        // chaque guilde dirigee par un de ses personnages disparait avec lui
        foreach (var ledGuild in await guilds.GetGuildsLedByPlayerAsync(player.Id, cancellationToken))
        {
            guilds.RemoveGuild(ledGuild);
        }

        players.Remove(player);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DeletePlayerOutcome.Deleted;
    }
}
