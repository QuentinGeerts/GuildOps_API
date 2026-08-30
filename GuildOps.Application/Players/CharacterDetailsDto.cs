using GuildOps.Application.Games;
using GuildOps.Domain.Players;

namespace GuildOps.Application.Players;

public sealed record CharacterDetailsDto(
    Guid Id,
    Guid GameId,
    Guid CharacterClassId,
    string Name,
    string Server,
    int Level,
    DateTimeOffset CreatedAt,
    IReadOnlyList<GameRoleDto> Roles,
    IReadOnlyList<AvailabilitySlotDto> Availabilities)
{
    public static CharacterDetailsDto From(Character character)
        => new(character.Id, character.GameId, character.CharacterClassId,
               character.Name, character.Server, character.Level, character.CreatedAt,
               [.. character.Roles.Select(assignment => GameRoleDto.From(assignment.GameRole!))
                                  .OrderBy(role => role.Name)],
               [.. character.Availabilities.OrderBy(availability => availability.Day)
                                           .ThenBy(availability => availability.Slot)
                                           .Select(AvailabilitySlotDto.From)]);
}
