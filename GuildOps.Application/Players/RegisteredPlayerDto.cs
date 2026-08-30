using GuildOps.Domain.Players;

namespace GuildOps.Application.Players;

public sealed record RegisteredPlayerDto(Guid Id, string AccountName, DateTimeOffset CreatedAt)
{
    public static RegisteredPlayerDto From(Player player)
        => new(player.Id, player.AccountName, player.CreatedAt);
}
