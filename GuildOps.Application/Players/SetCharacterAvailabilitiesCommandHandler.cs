using GuildOps.Application.Abstractions;
using GuildOps.Domain.Players;

namespace GuildOps.Application.Players;

internal sealed class SetCharacterAvailabilitiesCommandHandler(
    IPlayerRepository players,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SetCharacterAvailabilitiesCommand, SetCharacterAvailabilitiesOutcome>
{
    public async Task<SetCharacterAvailabilitiesOutcome> HandleAsync(SetCharacterAvailabilitiesCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Slots.Any(slot => !Enum.IsDefined(slot.Day) || !Enum.IsDefined(slot.Slot)))
        {
            return SetCharacterAvailabilitiesOutcome.InvalidSlot;
        }

        var character = await players.GetCharacterForUpdateAsync(command.CharacterId, cancellationToken);

        if (character is null || character.PlayerId != command.PlayerId)
        {
            return SetCharacterAvailabilitiesOutcome.CharacterNotFound;
        }

        List<AvailabilitySlotDto> requested = [.. command.Slots.Distinct()];

        foreach (var obsolete in character.Availabilities
                     .Where(availability => !requested.Any(slot => slot.Day == availability.Day && slot.Slot == availability.Slot))
                     .ToList())
        {
            players.RemoveAvailability(obsolete);
        }

        foreach (var slot in requested.Where(slot =>
                     character.Availabilities.All(availability => availability.Day != slot.Day || availability.Slot != slot.Slot)))
        {
            players.AddAvailability(new Availability(character.Id, slot.Day, slot.Slot));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return SetCharacterAvailabilitiesOutcome.Updated;
    }
}
