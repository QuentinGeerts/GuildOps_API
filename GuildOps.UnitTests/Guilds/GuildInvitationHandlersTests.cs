using GuildOps.Application.Abstractions;
using GuildOps.Application.Guilds;
using GuildOps.Domain.Guilds;
using GuildOps.Domain.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Guilds;

public class AcceptGuildInvitationCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();
    private static readonly Guid GuildId = Guid.CreateVersion7();
    private static readonly Guid DefaultRankId = Guid.CreateVersion7();

    private readonly IPlayerRepository _players = Substitute.For<IPlayerRepository>();
    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private AcceptGuildInvitationCommandHandler Handler => new(_players, _guilds, _unitOfWork);

    private readonly Character _character =
        new(PlayerId, Guid.CreateVersion7(), Guid.CreateVersion7(), "Sylvane", "Hyjal", 80);

    private void CharacterIsLoaded()
        => _players.GetCharacterAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(_character);

    private GuildInvitation InvitationExists()
    {
        var invitation = new GuildInvitation(GuildId, _character.Id);
        _guilds.GetInvitationAsync(GuildId, _character.Id, Arg.Any<CancellationToken>()).Returns(invitation);
        return invitation;
    }

    private AcceptGuildInvitationCommand Command() => new(PlayerId, GuildId, _character.Id);

    [Fact]
    public async Task WhenCharacterIsUnknown_ReturnsCharacterNotOwned()
    {
        Assert.Equal(AcceptGuildInvitationOutcome.CharacterNotOwned, await Handler.HandleAsync(Command()));
    }

    /// <summary>Meme un grade de la guilde ne peut pas accepter a la place de l'invite.</summary>
    [Fact]
    public async Task WhenCharacterBelongsToAnotherPlayer_ReturnsCharacterNotOwned()
    {
        CharacterIsLoaded();

        var outcome = await Handler.HandleAsync(new AcceptGuildInvitationCommand(Guid.CreateVersion7(), GuildId, _character.Id));

        Assert.Equal(AcceptGuildInvitationOutcome.CharacterNotOwned, outcome);
    }

    [Fact]
    public async Task WhenNoInvitationIsPending_ReturnsInvitationNotFound()
    {
        CharacterIsLoaded();

        Assert.Equal(AcceptGuildInvitationOutcome.InvitationNotFound, await Handler.HandleAsync(Command()));
    }

    [Fact]
    public async Task WhenCharacterAlreadyBelongsToAGuild_ReturnsCharacterAlreadyInGuild()
    {
        CharacterIsLoaded();
        InvitationExists();
        _guilds.CharacterHasMembershipAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(true);

        Assert.Equal(AcceptGuildInvitationOutcome.CharacterAlreadyInGuild, await Handler.HandleAsync(Command()));
    }

    [Fact]
    public async Task WhenTheGuildHasNoDefaultRank_ReturnsNoDefaultRank()
    {
        CharacterIsLoaded();
        InvitationExists();

        Assert.Equal(AcceptGuildInvitationOutcome.NoDefaultRank, await Handler.HandleAsync(Command()));
    }

    [Fact]
    public async Task WhenEverythingIsValid_JoinsAtTheDefaultRankAndDropsTheInvitation()
    {
        CharacterIsLoaded();
        var invitation = InvitationExists();
        _guilds.GetDefaultRankIdAsync(GuildId, Arg.Any<CancellationToken>()).Returns(DefaultRankId);

        var outcome = await Handler.HandleAsync(Command());

        Assert.Equal(AcceptGuildInvitationOutcome.Accepted, outcome);
        _guilds.Received(1).AddMembership(Arg.Is<GuildMembership>(m => m.GuildRankId == DefaultRankId));
        _guilds.Received(1).RemoveInvitation(invitation);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class DeclineGuildInvitationCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();
    private static readonly Guid GuildId = Guid.CreateVersion7();

    private readonly IPlayerRepository _players = Substitute.For<IPlayerRepository>();
    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private DeclineGuildInvitationCommandHandler Handler => new(_players, _guilds, _unitOfWork);

    private readonly Character _character =
        new(PlayerId, Guid.CreateVersion7(), Guid.CreateVersion7(), "Sylvane", "Hyjal", 80);

    private GuildInvitation InvitationExists()
    {
        var invitation = new GuildInvitation(GuildId, _character.Id);
        _guilds.GetInvitationAsync(GuildId, _character.Id, Arg.Any<CancellationToken>()).Returns(invitation);
        return invitation;
    }

    [Fact]
    public async Task WhenNoInvitationIsPending_ReturnsInvitationNotFound()
    {
        var outcome = await Handler.HandleAsync(new DeclineGuildInvitationCommand(PlayerId, GuildId, _character.Id));

        Assert.Equal(DeclineGuildInvitationOutcome.InvitationNotFound, outcome);
    }

    [Fact]
    public async Task WhenCallerNeitherOwnsTheCharacterNorMayInvite_ReturnsForbidden()
    {
        InvitationExists();

        var outcome = await Handler.HandleAsync(new DeclineGuildInvitationCommand(Guid.CreateVersion7(), GuildId, _character.Id));

        Assert.Equal(DeclineGuildInvitationOutcome.Forbidden, outcome);
        _guilds.DidNotReceive().RemoveInvitation(Arg.Any<GuildInvitation>());
    }

    /// <summary>Premier acteur : l'invite decline lui-meme.</summary>
    [Fact]
    public async Task WhenCallerOwnsTheCharacter_DropsTheInvitationWithoutCheckingPermissions()
    {
        var invitation = InvitationExists();
        _players.GetCharacterAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(_character);

        var outcome = await Handler.HandleAsync(new DeclineGuildInvitationCommand(PlayerId, GuildId, _character.Id));

        Assert.Equal(DeclineGuildInvitationOutcome.Declined, outcome);
        _guilds.Received(1).RemoveInvitation(invitation);
        await _guilds.DidNotReceive().HasPermissionAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<GuildPermission>(), Arg.Any<CancellationToken>());
    }

    /// <summary>Second acteur : un grade annule l'invitation qu'il a emise.</summary>
    [Fact]
    public async Task WhenCallerMayInvite_DropsTheInvitation()
    {
        var invitation = InvitationExists();
        Guid officerId = Guid.CreateVersion7();
        _guilds.HasPermissionAsync(GuildId, officerId, GuildPermission.InviteMember, Arg.Any<CancellationToken>()).Returns(true);

        var outcome = await Handler.HandleAsync(new DeclineGuildInvitationCommand(officerId, GuildId, _character.Id));

        Assert.Equal(DeclineGuildInvitationOutcome.Declined, outcome);
        _guilds.Received(1).RemoveInvitation(invitation);
    }
}

public class GetGuildInvitationsQueryHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();
    private static readonly Guid GuildId = Guid.CreateVersion7();

    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();

    private GetGuildInvitationsQueryHandler Handler => new(_guilds);

    [Fact]
    public async Task WithoutPermission_ReturnsForbiddenAndDoesNotQuery()
    {
        var result = await Handler.HandleAsync(new GetGuildInvitationsQuery(PlayerId, GuildId));

        Assert.Equal(GuildInvitationsOutcome.Forbidden, result.Outcome);
        Assert.Empty(result.Invitations);
        await _guilds.DidNotReceive().GetInvitationsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WithPermission_ReturnsTheInvitations()
    {
        _guilds.HasPermissionAsync(GuildId, PlayerId, GuildPermission.InviteMember, Arg.Any<CancellationToken>()).Returns(true);

        var character = new Character(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), "Sylvane", "Hyjal", 80);
        var invitation = new GuildInvitation(GuildId, character.Id, "Rejoins-nous") { Character = character };
        _guilds.GetInvitationsAsync(GuildId, Arg.Any<CancellationToken>()).Returns([invitation]);

        var result = await Handler.HandleAsync(new GetGuildInvitationsQuery(PlayerId, GuildId));

        Assert.Equal(GuildInvitationsOutcome.Retrieved, result.Outcome);
        var dto = Assert.Single(result.Invitations);
        Assert.Equal("Sylvane", dto.CharacterName);
    }
}
