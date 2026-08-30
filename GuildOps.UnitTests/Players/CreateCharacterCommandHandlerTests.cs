using GuildOps.Application.Abstractions;
using GuildOps.Application.Players;
using GuildOps.Domain.Games;
using NSubstitute;

namespace GuildOps.UnitTests.Players;

public class CreateCharacterCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();

    private readonly IPlayerRepository _players = Substitute.For<IPlayerRepository>();
    private readonly IGameRepository _games = Substitute.For<IGameRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CreateCharacterCommandHandler Handler => new(_players, _games, _unitOfWork);

    private readonly Game _game = new("World of Warcraft", 80);
    private readonly CharacterClass _paladin;

    public CreateCharacterCommandHandlerTests()
    {
        _paladin = new CharacterClass(_game.Id, "Paladin", 1);
        _game.Classes.Add(_paladin);
    }

    private void PlayerExists()
        => _players.ExistsAsync(PlayerId, Arg.Any<CancellationToken>()).Returns(true);

    private void GameExists()
        => _games.GetWithClassesAndRolesAsync(_game.Id, Arg.Any<CancellationToken>()).Returns(_game);

    private CreateCharacterCommand Command(int level = 80, Guid? classId = null)
        => new(PlayerId, _game.Id, classId ?? _paladin.Id, "Kaelis", "Hyjal", level);

    [Fact]
    public async Task WhenPlayerDoesNotExist_ReturnsPlayerNotFound()
    {
        var result = await Handler.HandleAsync(Command());

        Assert.Equal(CreateCharacterOutcome.PlayerNotFound, result.Outcome);
    }

    [Fact]
    public async Task WhenGameDoesNotExist_ReturnsGameNotFound()
    {
        PlayerExists();

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(CreateCharacterOutcome.GameNotFound, result.Outcome);
    }

    [Fact]
    public async Task WhenClassBelongsToAnotherGame_ReturnsClassNotInGame()
    {
        PlayerExists();
        GameExists();

        var result = await Handler.HandleAsync(Command(classId: Guid.CreateVersion7()));

        Assert.Equal(CreateCharacterOutcome.ClassNotInGame, result.Outcome);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(81)]
    public async Task WhenLevelIsOutsideTheGameBounds_ReturnsLevelOutOfRange(int level)
    {
        PlayerExists();
        GameExists();

        var result = await Handler.HandleAsync(Command(level));

        Assert.Equal(CreateCharacterOutcome.LevelOutOfRange, result.Outcome);
    }

    [Fact]
    public async Task WhenNameIsTakenOnTheServer_ReturnsNameTakenOnServer()
    {
        PlayerExists();
        GameExists();
        _players.CharacterNameExistsAsync("Hyjal", "Kaelis", Arg.Any<CancellationToken>()).Returns(true);

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(CreateCharacterOutcome.NameTakenOnServer, result.Outcome);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenEverythingIsValid_AddsTheCharacterAndSaves()
    {
        PlayerExists();
        GameExists();

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(CreateCharacterOutcome.Created, result.Outcome);
        Assert.NotNull(result.Character);
        Assert.Equal("Kaelis", result.Character!.Name);
        _players.Received(1).AddCharacter(Arg.Is<GuildOps.Domain.Players.Character>(c => c.Name == "Kaelis" && c.PlayerId == PlayerId));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
