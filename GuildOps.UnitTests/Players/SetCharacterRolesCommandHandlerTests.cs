using GuildOps.Application.Abstractions;
using GuildOps.Application.Players;
using GuildOps.Domain.Games;
using GuildOps.Domain.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Players;

public class SetCharacterRolesCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();

    private readonly IPlayerRepository _players = Substitute.For<IPlayerRepository>();
    private readonly IGameRepository _games = Substitute.For<IGameRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private SetCharacterRolesCommandHandler Handler => new(_players, _games, _unitOfWork);

    private readonly Game _game = new("World of Warcraft", 80);
    private readonly GameRole _tank;
    private readonly GameRole _dps;
    private readonly Character _character;

    public SetCharacterRolesCommandHandlerTests()
    {
        _tank = new GameRole(_game.Id, "Tank", 1);
        _dps = new GameRole(_game.Id, "DPS", 2);
        _game.Roles.Add(_tank);
        _game.Roles.Add(_dps);

        _character = new Character(PlayerId, _game.Id, Guid.CreateVersion7(), "Kaelis", "Hyjal", 80);
    }

    private void CharacterIsLoaded()
    {
        _players.GetCharacterForUpdateAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(_character);
        _games.GetWithClassesAndRolesAsync(_game.Id, Arg.Any<CancellationToken>()).Returns(_game);
    }

    private SetCharacterRolesCommand Command(params Guid[] roleIds)
        => new(PlayerId, _character.Id, roleIds);

    [Fact]
    public async Task WhenCharacterIsUnknown_ReturnsCharacterNotFound()
    {
        var outcome = await Handler.HandleAsync(Command(_tank.Id));

        Assert.Equal(SetCharacterRolesOutcome.CharacterNotFound, outcome);
    }

    [Fact]
    public async Task WhenCharacterBelongsToAnotherPlayer_ReturnsCharacterNotFound()
    {
        _players.GetCharacterForUpdateAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(_character);

        var outcome = await Handler.HandleAsync(new SetCharacterRolesCommand(Guid.CreateVersion7(), _character.Id, [_tank.Id]));

        Assert.Equal(SetCharacterRolesOutcome.CharacterNotFound, outcome);
    }

    [Fact]
    public async Task WhenARoleBelongsToAnotherGame_ReturnsRoleNotInGame()
    {
        CharacterIsLoaded();

        var outcome = await Handler.HandleAsync(Command(_tank.Id, Guid.CreateVersion7()));

        Assert.Equal(SetCharacterRolesOutcome.RoleNotInGame, outcome);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenRolesAreNew_AddsThemAll()
    {
        CharacterIsLoaded();

        var outcome = await Handler.HandleAsync(Command(_tank.Id, _dps.Id));

        Assert.Equal(SetCharacterRolesOutcome.Updated, outcome);
        _players.Received(1).AddCharacterRole(Arg.Is<CharacterGameRole>(a => a.GameRoleId == _tank.Id));
        _players.Received(1).AddCharacterRole(Arg.Is<CharacterGameRole>(a => a.GameRoleId == _dps.Id));
        _players.DidNotReceive().RemoveCharacterRole(Arg.Any<CharacterGameRole>());
    }

    [Fact]
    public async Task WhenTheSameListIsSentAgain_ChangesNothing()
    {
        _character.Roles.Add(new CharacterGameRole(_character.Id, _tank.Id));
        CharacterIsLoaded();

        var outcome = await Handler.HandleAsync(Command(_tank.Id));

        Assert.Equal(SetCharacterRolesOutcome.Updated, outcome);
        _players.DidNotReceive().AddCharacterRole(Arg.Any<CharacterGameRole>());
        _players.DidNotReceive().RemoveCharacterRole(Arg.Any<CharacterGameRole>());
    }

    [Fact]
    public async Task WhenTheListShrinks_RemovesOnlyWhatIsMissing()
    {
        var keptRole = new CharacterGameRole(_character.Id, _tank.Id);
        var droppedRole = new CharacterGameRole(_character.Id, _dps.Id);
        _character.Roles.Add(keptRole);
        _character.Roles.Add(droppedRole);
        CharacterIsLoaded();

        var outcome = await Handler.HandleAsync(Command(_tank.Id));

        Assert.Equal(SetCharacterRolesOutcome.Updated, outcome);
        _players.Received(1).RemoveCharacterRole(droppedRole);
        _players.DidNotReceive().RemoveCharacterRole(keptRole);
        _players.DidNotReceive().AddCharacterRole(Arg.Any<CharacterGameRole>());
    }

    [Fact]
    public async Task WhenTheListIsEmpty_RemovesEverything()
    {
        _character.Roles.Add(new CharacterGameRole(_character.Id, _tank.Id));
        CharacterIsLoaded();

        var outcome = await Handler.HandleAsync(Command());

        Assert.Equal(SetCharacterRolesOutcome.Updated, outcome);
        _players.Received(1).RemoveCharacterRole(Arg.Any<CharacterGameRole>());
    }
}
