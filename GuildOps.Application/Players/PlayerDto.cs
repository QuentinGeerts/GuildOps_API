using GuildOps.Domain.Players;

namespace GuildOps.Application.Players;

public sealed record PlayerDto(
    Guid Id,
    string AccountName,
    DateTimeOffset CreatedAt,
    IReadOnlyList<CharacterDto> Characters)
{
    public static PlayerDto From(Player player)
        => new(player.Id, player.AccountName, player.CreatedAt,
               [.. player.Characters.OrderBy(character => character.Name).Select(CharacterDto.From)]);
}
