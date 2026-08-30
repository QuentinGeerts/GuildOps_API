namespace GuildOps.Domain.Players;

public class Availability
{
    private Availability() { }

    public Availability(Guid characterId, DayOfWeek day, TimeSlot slot)
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTimeOffset.UtcNow;
        CharacterId = characterId;
        Day = day;
        Slot = slot;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CharacterId { get; set; }
    public Character? Character { get; set; }

    public DayOfWeek Day { get; set; }
    public TimeSlot Slot { get; set; }
}
