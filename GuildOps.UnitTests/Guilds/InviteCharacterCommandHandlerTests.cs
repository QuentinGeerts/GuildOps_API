using GuildOps.Application.Abstractions;
using GuildOps.Application.Guilds;
using GuildOps.Domain.Guilds;
using GuildOps.Domain.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Guilds;

public class InviteCharacterCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();
    private static readonly Guid GameId = Guid.CreateVersion7();

    private readonly IPlayerRepository _players = Substitute.For<IPlayerRepository>();
    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private InviteCharacterCommandHandler Handler => new(_players, _guilds, _unitOfWork);

    private readonly Guild _guild = new(GameId, "Les Veilleurs", "Hyjal");

    private readonly Character _character =
        new(Guid.CreateVersion7(), GameId, Guid.CreateVersion7(), "Sylvane", "Hyjal", 80);

    private void GrantPermission()
        => _guilds.HasPermissionAsync(_guild.Id, PlayerId, GuildPermission.InviteMember, Arg.Any<CancellationToken>())
                  .Returns(true);

    private void GuildIsLoaded()
        => _guilds.GetAsync(_guild.Id, Arg.Any<CancellationToken>()).Returns(_guild);

    private void CharacterIsLoaded(Character? character = null)
        => _players.GetCharacterAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(character ?? _character);

    private InviteCharacterCommand Command() => new(PlayerId, _guild.Id, _character.Id, "Rejoins-nous");

    [Fact]
    public async Task WithoutPermission_ReturnsForbiddenBeforeAnyLookup()
    {
        var result = await Handler.HandleAsync(Command());

        Assert.Equal(InviteCharacterOutcome.Forbidden, result.Outcome);
        await _guilds.DidNotReceive().GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _players.DidNotReceive().GetCharacterAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenGuildIsUnknown_ReturnsGuildNotFound()
    {
        GrantPermission();

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(InviteCharacterOutcome.GuildNotFound, result.Outcome);
    }

    [Fact]
    public async Task WhenCharacterIsUnknown_ReturnsCharacterNotFound()
    {
        GrantPermission();
        GuildIsLoaded();

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(InviteCharacterOutcome.CharacterNotFound, result.Outcome);
    }

    [Fact]
    public async Task WhenCharacterIsOnAnotherServer_ReturnsDifferentGameOrServer()
    {
        GrantPermission();
        GuildIsLoaded();
        CharacterIsLoaded(new Character(Guid.CreateVersion7(), GameId, Guid.CreateVersion7(), "Etranger", "KaelThas", 80));
        _players.GetCharacterAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(new Character(Guid.CreateVersion7(), GameId, Guid.CreateVersion7(), "Etranger", "KaelThas", 80));

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(InviteCharacterOutcome.DifferentGameOrServer, result.Outcome);
    }

    [Fact]
    public async Task WhenCharacterAlreadyBelongsToAGuild_ReturnsCharacterAlreadyInGuild()
    {
        GrantPermission();
        GuildIsLoaded();
        CharacterIsLoaded();
        _guilds.CharacterHasMembershipAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(InviteCharacterOutcome.CharacterAlreadyInGuild, result.Outcome);
    }

    [Fact]
    public async Task WhenAnInvitationIsAlreadyPending_ReturnsAlreadyInvited()
    {
        GrantPermission();
        GuildIsLoaded();
        CharacterIsLoaded();
        _guilds.InvitationExistsAsync(_guild.Id, _character.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(InviteCharacterOutcome.AlreadyInvited, result.Outcome);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenTheUniqueIndexIsViolated_ReturnsAlreadyInvited()
    {
        GrantPermission();
        GuildIsLoaded();
        CharacterIsLoaded();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
                   .Returns<Task<int>>(_ => throw new UniqueConstraintException("IX_GuildInvitations_GuildId_CharacterId", new Exception()));

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(InviteCharacterOutcome.AlreadyInvited, result.Outcome);
    }

    [Fact]
    public async Task WhenEverythingIsValid_AddsTheInvitation()
    {
        GrantPermission();
        GuildIsLoaded();
        CharacterIsLoaded();

        var result = await Handler.HandleAsync(Command());

        Assert.Equal(InviteCharacterOutcome.Created, result.Outcome);
        Assert.Equal("Sylvane", result.Invitation!.CharacterName);
        _guilds.Received(1).AddInvitation(Arg.Is<GuildInvitation>(i => i.CharacterId == _character.Id));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
