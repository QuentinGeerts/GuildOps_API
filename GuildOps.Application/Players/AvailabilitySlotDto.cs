using GuildOps.Domain.Players;

namespace GuildOps.Application.Players;

public sealed record AvailabilitySlotDto(DayOfWeek Day, TimeSlot Slot)
{
    public static AvailabilitySlotDto From(Availability availability)
        => new(availability.Day, availability.Slot);
}
