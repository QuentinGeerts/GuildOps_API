using GuildOps.Domain.Games;

namespace GuildOps.Application.Games;

public sealed record CharacterClassDto(Guid Id, string Name)
{
    public static CharacterClassDto From(CharacterClass characterClass)
        => new(characterClass.Id, characterClass.Name);
}
