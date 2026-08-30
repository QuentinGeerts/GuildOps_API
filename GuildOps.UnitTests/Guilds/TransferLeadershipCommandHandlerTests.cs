using GuildOps.Application.Abstractions;
using GuildOps.Application.Guilds;
using GuildOps.Domain.Games;
using GuildOps.Domain.Guilds;
using GuildOps.Domain.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Guilds;

public class TransferLeadershipCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();
    private static readonly Guid GuildId = Guid.CreateVersion7();
    private static readonly Guid LeaderRankId = Guid.CreateVersion7();
    private static readonly Guid MemberRankId = Guid.CreateVersion7();

    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private TransferLeadershipCommandHandler Handler => new(_guilds, _unitOfWork);

    private static Character Character(Guid playerId)
        => new(playerId, Guid.CreateVersion7(), Guid.CreateVersion7(), "Kaelis", "Hyjal", 80);

    private GuildMembership Leadership(Guid ownerId)
    {
        var character = Character(ownerId);
        var membership = new GuildMembership(GuildId, character.Id, LeaderRankId) { Character = character };
        _guilds.GetLeaderMembershipAsync(GuildId, Arg.Any<CancellationToken>()).Returns(membership);
        return membership;
    }

    [Fact]
    public async Task WhenGuildHasNoLeader_ReturnsNotLeader()
    {
        var outcome = await Handler.HandleAsync(new TransferLeadershipCommand(PlayerId, GuildId, Guid.CreateVersion7()));

        Assert.Equal(TransferLeadershipOutcome.NotLeader, outcome);
    }

    [Fact]
    public async Task WhenCallerIsNotTheLeader_ReturnsNotLeader()
    {
        Leadership(Guid.CreateVersion7());

        var outcome = await Handler.HandleAsync(new TransferLeadershipCommand(PlayerId, GuildId, Guid.CreateVersion7()));

        Assert.Equal(TransferLeadershipOutcome.NotLeader, outcome);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenTransferringToOneself_ReturnsAlreadyLeader()
    {
        var leadership = Leadership(PlayerId);

        var outcome = await Handler.HandleAsync(new TransferLeadershipCommand(PlayerId, GuildId, leadership.CharacterId));

        Assert.Equal(TransferLeadershipOutcome.AlreadyLeader, outcome);
    }

    [Fact]
    public async Task WhenSuccessorIsNotAMember_ReturnsMembershipNotFound()
    {
        Leadership(PlayerId);
        Guid successorId = Guid.CreateVersion7();

        var outcome = await Handler.HandleAsync(new TransferLeadershipCommand(PlayerId, GuildId, successorId));

        Assert.Equal(TransferLeadershipOutcome.MembershipNotFound, outcome);
    }

    [Fact]
    public async Task WhenEverythingIsValid_SwapsBothRanksAndSaves()
    {
        var leadership = Leadership(PlayerId);
        Guid successorId = Guid.CreateVersion7();
        var successor = new GuildMembership(GuildId, successorId, MemberRankId);
        _guilds.GetMembershipAsync(GuildId, successorId, Arg.Any<CancellationToken>()).Returns(successor);

        var outcome = await Handler.HandleAsync(new TransferLeadershipCommand(PlayerId, GuildId, successorId));

        Assert.Equal(TransferLeadershipOutcome.Transferred, outcome);
        Assert.Equal(LeaderRankId, successor.GuildRankId);
        Assert.Equal(MemberRankId, leadership.GuildRankId);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
