using GuildOps.Application.Abstractions;
using GuildOps.Application.Guilds;
using GuildOps.Domain.Guilds;
using GuildOps.Domain.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Guilds;

public class ApplyToGuildCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();
    private static readonly Guid GameId = Guid.CreateVersion7();

    private readonly IPlayerRepository _players = Substitute.For<IPlayerRepository>();
    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ApplyToGuildCommandHandler Handler => new(_players, _guilds, _unitOfWork);

    private readonly Character _character =
        new(PlayerId, GameId, Guid.CreateVersion7(), "Sylvane", "Hyjal", 80);

    private readonly Guild _guild = new(GameId, "Les Sentinelles", "Hyjal");

    private void CharacterIsLoaded()
        => _players.GetCharacterAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(_character);

    private void GuildIsLoaded(Guild? guild = null)
        => _guilds.GetAsync(_guild.Id, Arg.Any<CancellationToken>()).Returns(guild ?? _guild);

    private ApplyToGuildCommand Command() => new(PlayerId, _guild.Id, _character.Id, "Bonjour");

    [Fact]
    public async Task WhenCharacterIsUnknown_ReturnsCharacterNotFound()
    {
        var result = await Handler.HandleAsync(Command());

        Assert.Equal(ApplyToGuildOutcome.CharacterNotFound, result.Outcome);
    }

    [Fact]
    public async Task WhenCharacterBelongsToAnotherPlayer_ReturnsCharacterNotFound()
    {
        CharacterIsLoaded();

        var result = await Handler.HandleAsync(new ApplyToGuildCommand(Guid.CreateVersion7(), _guild.Id, _character.Id, null));

        Assert.Equal(ApplyToGuildOutcome.CharacterNotFound, result.Outcome);
    }

    [Fact]
    public async Task WhenGuildIsUnknown_ReturnsGuildNotFound()
    {
        CharacterIsLoaded();

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(ApplyToGuildOutcome.GuildNotFound, result.Outcome);
    }

    [Fact]
    public async Task WhenGuildIsOnAnotherServer_ReturnsDifferentGameOrServer()
    {
        CharacterIsLoaded();
        GuildIsLoaded(new Guild(GameId, "Ailleurs", "Kael'Thas"));
        _guilds.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(new Guild(GameId, "Ailleurs", "KaelThas"));

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(ApplyToGuildOutcome.DifferentGameOrServer, result.Outcome);
    }

    [Fact]
    public async Task WhenGuildBelongsToAnotherGame_ReturnsDifferentGameOrServer()
    {
        CharacterIsLoaded();
        _guilds.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
               .Returns(new Guild(Guid.CreateVersion7(), "Autre jeu", "Hyjal"));

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(ApplyToGuildOutcome.DifferentGameOrServer, result.Outcome);
    }

    [Fact]
    public async Task WhenCharacterAlreadyBelongsToAGuild_ReturnsCharacterAlreadyInGuild()
    {
        CharacterIsLoaded();
        GuildIsLoaded();
        _guilds.CharacterHasMembershipAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(ApplyToGuildOutcome.CharacterAlreadyInGuild, result.Outcome);
    }

    [Fact]
    public async Task WhenAnApplicationIsAlreadyPending_ReturnsAlreadyApplied()
    {
        CharacterIsLoaded();
        GuildIsLoaded();
        _guilds.ApplicationExistsAsync(_guild.Id, _character.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(ApplyToGuildOutcome.AlreadyApplied, result.Outcome);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenTheUniqueIndexIsViolated_ReturnsAlreadyApplied()
    {
        CharacterIsLoaded();
        GuildIsLoaded();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
                   .Returns<Task<int>>(_ => throw new UniqueConstraintException("IX_GuildApplications_GuildId_CharacterId", new Exception()));

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(ApplyToGuildOutcome.AlreadyApplied, result.Outcome);
    }

    [Fact]
    public async Task WhenEverythingIsValid_AddsTheApplication()
    {
        CharacterIsLoaded();
        GuildIsLoaded();

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(ApplyToGuildOutcome.Created, result.Outcome);
        Assert.Equal("Sylvane", result.Application!.CharacterName);
        _guilds.Received(1).AddApplication(Arg.Is<GuildApplication>(a => a.CharacterId == _character.Id && a.Message == "Bonjour"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
