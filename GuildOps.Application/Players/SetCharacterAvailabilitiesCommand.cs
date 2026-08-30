using System.ComponentModel.DataAnnotations;

namespace GuildOps.Application.Players;

public sealed record SetCharacterAvailabilitiesRequest([Required] IReadOnlyList<AvailabilitySlotDto> Slots);

public sealed record SetCharacterAvailabilitiesCommand(Guid PlayerId, Guid CharacterId, IReadOnlyList<AvailabilitySlotDto> Slots)
{
    public static SetCharacterAvailabilitiesCommand From(Guid playerId, Guid characterId, SetCharacterAvailabilitiesRequest request)
        => new(playerId, characterId, request.Slots);
}

public enum SetCharacterAvailabilitiesOutcome
{
    Updated = 1,
    CharacterNotFound = 2,
    InvalidSlot = 3
}
