using GuildOps.Application.Abstractions;
using GuildOps.Application.Players;
using GuildOps.Domain.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Players;

public class SetCharacterAvailabilitiesCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();

    private readonly IPlayerRepository _players = Substitute.For<IPlayerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private SetCharacterAvailabilitiesCommandHandler Handler => new(_players, _unitOfWork);

    private readonly Character _character =
        new(PlayerId, Guid.CreateVersion7(), Guid.CreateVersion7(), "Kaelis", "Hyjal", 80);

    private void CharacterIsLoaded()
        => _players.GetCharacterForUpdateAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(_character);

    private SetCharacterAvailabilitiesCommand Command(params AvailabilitySlotDto[] slots)
        => new(PlayerId, _character.Id, slots);

    [Fact]
    public async Task WhenTheSlotIsOutsideTheEnum_ReturnsInvalidSlot()
    {
        var outcome = await Handler.HandleAsync(Command(new AvailabilitySlotDto(DayOfWeek.Tuesday, (TimeSlot)99)));

        Assert.Equal(SetCharacterAvailabilitiesOutcome.InvalidSlot, outcome);
    }

    [Fact]
    public async Task WhenTheDayIsOutsideTheEnum_ReturnsInvalidSlot()
    {
        var outcome = await Handler.HandleAsync(Command(new AvailabilitySlotDto((DayOfWeek)9, TimeSlot.Evening)));

        Assert.Equal(SetCharacterAvailabilitiesOutcome.InvalidSlot, outcome);
        await _players.DidNotReceive().GetCharacterForUpdateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenCharacterBelongsToAnotherPlayer_ReturnsCharacterNotFound()
    {
        CharacterIsLoaded();

        var outcome = await Handler.HandleAsync(
            new SetCharacterAvailabilitiesCommand(Guid.CreateVersion7(), _character.Id, []));

        Assert.Equal(SetCharacterAvailabilitiesOutcome.CharacterNotFound, outcome);
    }

    [Fact]
    public async Task WhenSlotsAreNew_AddsThemAll()
    {
        CharacterIsLoaded();

        var outcome = await Handler.HandleAsync(Command(
            new AvailabilitySlotDto(DayOfWeek.Tuesday, TimeSlot.Evening),
            new AvailabilitySlotDto(DayOfWeek.Saturday, TimeSlot.Morning)));

        Assert.Equal(SetCharacterAvailabilitiesOutcome.Updated, outcome);
        _players.Received(2).AddAvailability(Arg.Any<Availability>());
    }

    [Fact]
    public async Task WhenTheRequestContainsDuplicates_AddsOnlyOnce()
    {
        CharacterIsLoaded();
        var slot = new AvailabilitySlotDto(DayOfWeek.Tuesday, TimeSlot.Evening);

        await Handler.HandleAsync(Command(slot, slot));

        _players.Received(1).AddAvailability(Arg.Any<Availability>());
    }

    [Fact]
    public async Task WhenTheListIsEmpty_RemovesEverything()
    {
        var existing = new Availability(_character.Id, DayOfWeek.Tuesday, TimeSlot.Evening);
        _character.Availabilities.Add(existing);
        CharacterIsLoaded();

        var outcome = await Handler.HandleAsync(Command());

        Assert.Equal(SetCharacterAvailabilitiesOutcome.Updated, outcome);
        _players.Received(1).RemoveAvailability(existing);
    }
}
