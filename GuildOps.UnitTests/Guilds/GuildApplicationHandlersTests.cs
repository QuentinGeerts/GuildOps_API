using GuildOps.Application.Abstractions;
using GuildOps.Application.Guilds;
using GuildOps.Domain.Guilds;
using GuildOps.Domain.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Guilds;

public class AcceptGuildApplicationCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();
    private static readonly Guid GuildId = Guid.CreateVersion7();
    private static readonly Guid CharacterId = Guid.CreateVersion7();
    private static readonly Guid DefaultRankId = Guid.CreateVersion7();

    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private AcceptGuildApplicationCommandHandler Handler => new(_guilds, _unitOfWork);

    private static readonly AcceptGuildApplicationCommand Command = new(PlayerId, GuildId, CharacterId);

    private void GrantPermission()
        => _guilds.HasPermissionAsync(GuildId, PlayerId, GuildPermission.ReviewApplications, Arg.Any<CancellationToken>())
                  .Returns(true);

    private GuildApplication ApplicationExists()
    {
        var application = new GuildApplication(GuildId, CharacterId);
        _guilds.GetApplicationAsync(GuildId, CharacterId, Arg.Any<CancellationToken>()).Returns(application);
        return application;
    }

    private void HasDefaultRank()
        => _guilds.GetDefaultRankIdAsync(GuildId, Arg.Any<CancellationToken>()).Returns(DefaultRankId);

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden()
    {
        Assert.Equal(AcceptGuildApplicationOutcome.Forbidden, await Handler.HandleAsync(Command));
    }

    [Fact]
    public async Task WhenNoApplicationIsPending_ReturnsApplicationNotFound()
    {
        GrantPermission();

        Assert.Equal(AcceptGuildApplicationOutcome.ApplicationNotFound, await Handler.HandleAsync(Command));
    }

    [Fact]
    public async Task WhenCharacterJoinedAnotherGuildMeanwhile_ReturnsCharacterAlreadyInGuild()
    {
        GrantPermission();
        ApplicationExists();
        _guilds.CharacterHasMembershipAsync(CharacterId, Arg.Any<CancellationToken>()).Returns(true);

        Assert.Equal(AcceptGuildApplicationOutcome.CharacterAlreadyInGuild, await Handler.HandleAsync(Command));
    }

    [Fact]
    public async Task WhenTheGuildHasNoDefaultRank_ReturnsNoDefaultRank()
    {
        GrantPermission();
        ApplicationExists();

        Assert.Equal(AcceptGuildApplicationOutcome.NoDefaultRank, await Handler.HandleAsync(Command));
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenEverythingIsValid_CreatesTheMembershipAtTheDefaultRankAndDropsTheApplication()
    {
        GrantPermission();
        var application = ApplicationExists();
        HasDefaultRank();

        var outcome = await Handler.HandleAsync(Command);

        Assert.Equal(AcceptGuildApplicationOutcome.Accepted, outcome);
        _guilds.Received(1).AddMembership(Arg.Is<GuildMembership>(m => m.CharacterId == CharacterId && m.GuildRankId == DefaultRankId));
        _guilds.Received(1).RemoveApplication(application);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class RejectGuildApplicationCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();
    private static readonly Guid GuildId = Guid.CreateVersion7();
    private static readonly Guid CharacterId = Guid.CreateVersion7();

    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private RejectGuildApplicationCommandHandler Handler => new(_guilds, _unitOfWork);

    private static readonly RejectGuildApplicationCommand Command = new(PlayerId, GuildId, CharacterId);

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden()
    {
        Assert.Equal(RejectGuildApplicationOutcome.Forbidden, await Handler.HandleAsync(Command));
        _guilds.DidNotReceive().RemoveApplication(Arg.Any<GuildApplication>());
    }

    [Fact]
    public async Task WhenNoApplicationIsPending_ReturnsApplicationNotFound()
    {
        _guilds.HasPermissionAsync(GuildId, PlayerId, GuildPermission.ReviewApplications, Arg.Any<CancellationToken>()).Returns(true);

        Assert.Equal(RejectGuildApplicationOutcome.ApplicationNotFound, await Handler.HandleAsync(Command));
    }

    [Fact]
    public async Task WhenEverythingIsValid_DropsTheApplication()
    {
        _guilds.HasPermissionAsync(GuildId, PlayerId, GuildPermission.ReviewApplications, Arg.Any<CancellationToken>()).Returns(true);
        var application = new GuildApplication(GuildId, CharacterId);
        _guilds.GetApplicationAsync(GuildId, CharacterId, Arg.Any<CancellationToken>()).Returns(application);

        Assert.Equal(RejectGuildApplicationOutcome.Rejected, await Handler.HandleAsync(Command));
        _guilds.Received(1).RemoveApplication(application);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class GetGuildApplicationsQueryHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();
    private static readonly Guid GuildId = Guid.CreateVersion7();

    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();

    private GetGuildApplicationsQueryHandler Handler => new(_guilds);

    [Fact]
    public async Task WithoutPermission_ReturnsForbiddenAndDoesNotQuery()
    {
        var result = await Handler.HandleAsync(new GetGuildApplicationsQuery(PlayerId, GuildId));

        Assert.Equal(GuildApplicationsOutcome.Forbidden, result.Outcome);
        Assert.Empty(result.Applications);
        await _guilds.DidNotReceive().GetApplicationsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WithPermission_ReturnsTheApplications()
    {
        _guilds.HasPermissionAsync(GuildId, PlayerId, GuildPermission.ReviewApplications, Arg.Any<CancellationToken>()).Returns(true);

        var character = new Character(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), "Sylvane", "Hyjal", 80);
        var application = new GuildApplication(GuildId, character.Id, "Bonjour") { Character = character };
        _guilds.GetApplicationsAsync(GuildId, Arg.Any<CancellationToken>()).Returns([application]);

        var result = await Handler.HandleAsync(new GetGuildApplicationsQuery(PlayerId, GuildId));

        Assert.Equal(GuildApplicationsOutcome.Retrieved, result.Outcome);
        var dto = Assert.Single(result.Applications);
        Assert.Equal("Sylvane", dto.CharacterName);
        Assert.Equal("Bonjour", dto.Message);
    }
}
