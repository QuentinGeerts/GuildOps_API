using GuildOps.Application.Abstractions;
using GuildOps.Application.Guilds;
using GuildOps.Domain.Guilds;
using NSubstitute;

namespace GuildOps.UnitTests.Guilds;

public class AssignMemberRankCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();
    private static readonly Guid GuildId = Guid.CreateVersion7();
    private static readonly Guid CharacterId = Guid.CreateVersion7();

    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private AssignMemberRankCommandHandler Handler => new(_guilds, _unitOfWork);

    private void GrantPermission()
        => _guilds.HasPermissionAsync(GuildId, PlayerId, GuildPermission.AssignRank, Arg.Any<CancellationToken>())
                  .Returns(true);

    private static GuildRank Rank(string name, bool isLeader = false)
        => new(GuildId, name, 1, [GuildPermission.ViewMembers], isLeader);

    private void HasRank(GuildRank rank)
        => _guilds.GetRankAsync(GuildId, rank.Id, Arg.Any<CancellationToken>()).Returns(rank);

    private void HasMembership(GuildMembership membership)
        => _guilds.GetMembershipAsync(GuildId, CharacterId, Arg.Any<CancellationToken>()).Returns(membership);

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden()
    {
        var outcome = await Handler.HandleAsync(new AssignMemberRankCommand(PlayerId, GuildId, CharacterId, Guid.CreateVersion7()));

        Assert.Equal(AssignMemberRankOutcome.Forbidden, outcome);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenRankBelongsToAnotherGuild_ReturnsRankNotInGuild()
    {
        GrantPermission();

        var outcome = await Handler.HandleAsync(new AssignMemberRankCommand(PlayerId, GuildId, CharacterId, Guid.CreateVersion7()));

        Assert.Equal(AssignMemberRankOutcome.RankNotInGuild, outcome);
    }

    [Fact]
    public async Task WhenTargetRankIsLeader_ReturnsCannotAssignLeaderRank()
    {
        GrantPermission();
        var leaderRank = Rank("Chef de guilde", isLeader: true);
        HasRank(leaderRank);

        var outcome = await Handler.HandleAsync(new AssignMemberRankCommand(PlayerId, GuildId, CharacterId, leaderRank.Id));

        Assert.Equal(AssignMemberRankOutcome.CannotAssignLeaderRank, outcome);
    }

    [Fact]
    public async Task WhenCharacterIsNotAMember_ReturnsMembershipNotFound()
    {
        GrantPermission();
        var officer = Rank("Officier");
        HasRank(officer);

        var outcome = await Handler.HandleAsync(new AssignMemberRankCommand(PlayerId, GuildId, CharacterId, officer.Id));

        Assert.Equal(AssignMemberRankOutcome.MembershipNotFound, outcome);
    }

    /// <summary>Regression : retirer son grade au chef laissait la guilde sans chef.</summary>
    [Fact]
    public async Task WhenMemberIsTheLeader_ReturnsCannotDemoteLeader()
    {
        GrantPermission();
        var officer = Rank("Officier");
        var leaderRank = Rank("Chef de guilde", isLeader: true);
        HasRank(officer);
        HasRank(leaderRank);
        HasMembership(new GuildMembership(GuildId, CharacterId, leaderRank.Id));

        var outcome = await Handler.HandleAsync(new AssignMemberRankCommand(PlayerId, GuildId, CharacterId, officer.Id));

        Assert.Equal(AssignMemberRankOutcome.CannotDemoteLeader, outcome);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenEverythingIsValid_AssignsTheRankAndSaves()
    {
        GrantPermission();
        var officer = Rank("Officier");
        var member = Rank("Membre");
        HasRank(officer);
        HasRank(member);
        var membership = new GuildMembership(GuildId, CharacterId, member.Id);
        HasMembership(membership);

        var outcome = await Handler.HandleAsync(new AssignMemberRankCommand(PlayerId, GuildId, CharacterId, officer.Id));

        Assert.Equal(AssignMemberRankOutcome.Updated, outcome);
        Assert.Equal(officer.Id, membership.GuildRankId);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
