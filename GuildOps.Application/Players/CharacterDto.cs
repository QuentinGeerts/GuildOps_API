using GuildOps.Domain.Players;

namespace GuildOps.Application.Players;

public sealed record CharacterDto(
    Guid Id,
    Guid GameId,
    Guid CharacterClassId,
    string Name,
    string Server,
    int Level,
    DateTimeOffset CreatedAt)
{
    public static CharacterDto From(Character character)
        => new(character.Id, character.GameId, character.CharacterClassId,
               character.Name, character.Server, character.Level, character.CreatedAt);
}
