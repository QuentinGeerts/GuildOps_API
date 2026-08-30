using GuildOps.Application.Abstractions;
using GuildOps.Application.Guilds;
using GuildOps.Domain.Guilds;
using GuildOps.Domain.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Guilds;

public class CreateGuildCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();
    private static readonly Guid GameId = Guid.CreateVersion7();

    private readonly IPlayerRepository _players = Substitute.For<IPlayerRepository>();
    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CreateGuildCommandHandler Handler => new(_players, _guilds, _unitOfWork);

    private readonly Character _character =
        new(PlayerId, GameId, Guid.CreateVersion7(), "Kaelis", "Hyjal", 80);

    private void CharacterIsLoaded()
        => _players.GetCharacterAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(_character);

    private CreateGuildCommand Command() => new(PlayerId, _character.Id, "Les Sentinelles", "PvE", null);

    [Fact]
    public async Task WhenCharacterIsUnknown_ReturnsCharacterNotFound()
    {
        var result = await Handler.HandleAsync(Command());

        Assert.Equal(CreateGuildOutcome.CharacterNotFound, result.Outcome);
    }

    [Fact]
    public async Task WhenCharacterBelongsToAnotherPlayer_ReturnsCharacterNotFound()
    {
        CharacterIsLoaded();

        var result = await Handler.HandleAsync(new CreateGuildCommand(Guid.CreateVersion7(), _character.Id, "X", null, null));

        Assert.Equal(CreateGuildOutcome.CharacterNotFound, result.Outcome);
    }

    [Fact]
    public async Task WhenCharacterAlreadyBelongsToAGuild_ReturnsCharacterAlreadyInGuild()
    {
        CharacterIsLoaded();
        _guilds.CharacterHasMembershipAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(CreateGuildOutcome.CharacterAlreadyInGuild, result.Outcome);
    }

    [Fact]
    public async Task WhenNameIsTakenOnTheServer_ReturnsNameTakenOnServer()
    {
        CharacterIsLoaded();
        _guilds.NameExistsOnServerAsync("Hyjal", "Les Sentinelles", Arg.Any<CancellationToken>()).Returns(true);

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(CreateGuildOutcome.NameTakenOnServer, result.Outcome);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenEverythingIsValid_TakesGameAndServerFromTheFoundingCharacter()
    {
        CharacterIsLoaded();
        Guild? added = null;
        _guilds.When(repository => repository.Add(Arg.Any<Guild>())).Do(call => added = call.Arg<Guild>());

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(CreateGuildOutcome.Created, result.Outcome);
        Assert.NotNull(added);
        Assert.Equal(GameId, added!.GameId);
        Assert.Equal("Hyjal", added.Server);
    }

    [Fact]
    public async Task WhenEverythingIsValid_CreatesTheThreeBaseRanksAndTheLeaderMembership()
    {
        CharacterIsLoaded();
        Guild? added = null;
        _guilds.When(repository => repository.Add(Arg.Any<Guild>())).Do(call => added = call.Arg<Guild>());

        await Handler.HandleAsync(Command());

        Assert.Equal(3, added!.Ranks.Count);
        Assert.Single(added.Ranks, rank => rank.IsLeader);
        Assert.Single(added.Ranks, rank => rank.IsDefault);

        var membership = Assert.Single(added.Memberships);
        Assert.Equal(_character.Id, membership.CharacterId);
        Assert.Equal(added.Ranks.Single(rank => rank.IsLeader).Id, membership.GuildRankId);
    }

    [Fact]
    public async Task WhenEverythingIsValid_SavesOnce()
    {
        CharacterIsLoaded();

        await Handler.HandleAsync(Command());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
