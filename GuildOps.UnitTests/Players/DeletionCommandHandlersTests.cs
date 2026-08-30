using GuildOps.Application.Abstractions;
using GuildOps.Application.Players;
using GuildOps.Domain.Guilds;
using GuildOps.Domain.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Players;

public class DeleteCharacterCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();

    private readonly IPlayerRepository _players = Substitute.For<IPlayerRepository>();
    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private DeleteCharacterCommandHandler Handler => new(_players, _guilds, _unitOfWork);

    private readonly Character _character =
        new(PlayerId, Guid.CreateVersion7(), Guid.CreateVersion7(), "Kaelis", "Hyjal", 80);

    private void CharacterIsLoaded()
        => _players.GetCharacterForUpdateAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(_character);

    [Fact]
    public async Task WhenCharacterIsUnknown_ReturnsCharacterNotFound()
    {
        var outcome = await Handler.HandleAsync(new DeleteCharacterCommand(PlayerId, _character.Id));

        Assert.Equal(DeleteCharacterOutcome.CharacterNotFound, outcome);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenCharacterBelongsToAnotherPlayer_ReturnsCharacterNotFound()
    {
        CharacterIsLoaded();

        var outcome = await Handler.HandleAsync(new DeleteCharacterCommand(Guid.CreateVersion7(), _character.Id));

        Assert.Equal(DeleteCharacterOutcome.CharacterNotFound, outcome);
        _players.DidNotReceive().RemoveCharacter(Arg.Any<Character>());
    }

    [Fact]
    public async Task WhenCharacterLeadsNoGuild_RemovesOnlyTheCharacter()
    {
        CharacterIsLoaded();

        var outcome = await Handler.HandleAsync(new DeleteCharacterCommand(PlayerId, _character.Id));

        Assert.Equal(DeleteCharacterOutcome.Deleted, outcome);
        _players.Received(1).RemoveCharacter(_character);
        _guilds.DidNotReceive().RemoveGuild(Arg.Any<Guild>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>Pas de guilde orpheline : supprimer le chef emporte la guilde.</summary>
    [Fact]
    public async Task WhenCharacterLeadsAGuild_RemovesTheGuildToo()
    {
        CharacterIsLoaded();
        var guild = new Guild(Guid.CreateVersion7(), "Les Sentinelles", "Hyjal");
        _guilds.GetGuildLedByCharacterAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(guild);

        var outcome = await Handler.HandleAsync(new DeleteCharacterCommand(PlayerId, _character.Id));

        Assert.Equal(DeleteCharacterOutcome.Deleted, outcome);
        _guilds.Received(1).RemoveGuild(guild);
        _players.Received(1).RemoveCharacter(_character);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class DeletePlayerCommandHandlerTests
{
    private readonly IPlayerRepository _players = Substitute.For<IPlayerRepository>();
    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private DeletePlayerCommandHandler Handler => new(_players, _guilds, _unitOfWork);

    private readonly Player _player = new("quentin");

    private void PlayerIsLoaded()
        => _players.GetForUpdateAsync(_player.Id, Arg.Any<CancellationToken>()).Returns(_player);

    [Fact]
    public async Task WhenPlayerIsUnknown_ReturnsPlayerNotFound()
    {
        var outcome = await Handler.HandleAsync(new DeletePlayerCommand(_player.Id));

        Assert.Equal(DeletePlayerOutcome.PlayerNotFound, outcome);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenPlayerLeadsNoGuild_RemovesOnlyThePlayer()
    {
        PlayerIsLoaded();

        var outcome = await Handler.HandleAsync(new DeletePlayerCommand(_player.Id));

        Assert.Equal(DeletePlayerOutcome.Deleted, outcome);
        _players.Received(1).Remove(_player);
        _guilds.DidNotReceive().RemoveGuild(Arg.Any<Guild>());
    }

    /// <summary>Un compte peut diriger plusieurs guildes, une par personnage.</summary>
    [Fact]
    public async Task WhenPlayerLeadsSeveralGuilds_RemovesThemAll()
    {
        PlayerIsLoaded();
        var first = new Guild(Guid.CreateVersion7(), "Les Sentinelles", "Hyjal");
        var second = new Guild(Guid.CreateVersion7(), "Les Veilleurs", "KaelThas");
        _guilds.GetGuildsLedByPlayerAsync(_player.Id, Arg.Any<CancellationToken>()).Returns([first, second]);

        var outcome = await Handler.HandleAsync(new DeletePlayerCommand(_player.Id));

        Assert.Equal(DeletePlayerOutcome.Deleted, outcome);
        _guilds.Received(1).RemoveGuild(first);
        _guilds.Received(1).RemoveGuild(second);
        _players.Received(1).Remove(_player);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
