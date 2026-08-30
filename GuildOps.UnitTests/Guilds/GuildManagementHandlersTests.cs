using GuildOps.Application.Abstractions;
using GuildOps.Application.Guilds;
using GuildOps.Domain.Guilds;
using GuildOps.Domain.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Guilds;

public class EditGuildProfileCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();

    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private EditGuildProfileCommandHandler Handler => new(_guilds, _unitOfWork);

    private readonly Guild _guild = new(Guid.CreateVersion7(), "Les Gardiens", "Hyjal");

    private void GrantPermission()
        => _guilds.HasPermissionAsync(_guild.Id, PlayerId, GuildPermission.EditGuildProfile, Arg.Any<CancellationToken>())
                  .Returns(true);

    private EditGuildProfileCommand Command()
        => new(PlayerId, _guild.Id, "Les Gardiens du Nord", "PvE HL", "https://discord.gg/x");

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden()
    {
        Assert.Equal(EditGuildProfileOutcome.Forbidden, await Handler.HandleAsync(Command()));
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenGuildIsUnknown_ReturnsGuildNotFound()
    {
        GrantPermission();

        Assert.Equal(EditGuildProfileOutcome.GuildNotFound, await Handler.HandleAsync(Command()));
    }

    [Fact]
    public async Task WhenTheNameIndexIsViolated_ReturnsNameTakenOnServer()
    {
        GrantPermission();
        _guilds.GetForUpdateAsync(_guild.Id, Arg.Any<CancellationToken>()).Returns(_guild);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
                   .Returns<Task<int>>(_ => throw new UniqueConstraintException("IX_Guilds_Server_Name", new Exception()));

        Assert.Equal(EditGuildProfileOutcome.NameTakenOnServer, await Handler.HandleAsync(Command()));
    }

    [Fact]
    public async Task WhenEverythingIsValid_UpdatesTheThreeFields()
    {
        GrantPermission();
        _guilds.GetForUpdateAsync(_guild.Id, Arg.Any<CancellationToken>()).Returns(_guild);

        var outcome = await Handler.HandleAsync(Command());

        Assert.Equal(EditGuildProfileOutcome.Updated, outcome);
        Assert.Equal("Les Gardiens du Nord", _guild.Name);
        Assert.Equal("PvE HL", _guild.Description);
        Assert.Equal("https://discord.gg/x", _guild.ChatUrl);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class SetMemberNoteCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();
    private static readonly Guid GuildId = Guid.CreateVersion7();
    private static readonly Guid CharacterId = Guid.CreateVersion7();

    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private SetMemberNoteCommandHandler Handler => new(_guilds, _unitOfWork);

    private void GrantPermission()
        => _guilds.HasPermissionAsync(GuildId, PlayerId, GuildPermission.WriteMemberNote, Arg.Any<CancellationToken>())
                  .Returns(true);

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden()
    {
        var outcome = await Handler.HandleAsync(new SetMemberNoteCommand(PlayerId, GuildId, CharacterId, "note"));

        Assert.Equal(SetMemberNoteOutcome.Forbidden, outcome);
    }

    [Fact]
    public async Task WhenCharacterIsNotAMember_ReturnsMembershipNotFound()
    {
        GrantPermission();

        var outcome = await Handler.HandleAsync(new SetMemberNoteCommand(PlayerId, GuildId, CharacterId, "note"));

        Assert.Equal(SetMemberNoteOutcome.MembershipNotFound, outcome);
    }

    [Fact]
    public async Task WhenEverythingIsValid_WritesTheNote()
    {
        GrantPermission();
        var membership = new GuildMembership(GuildId, CharacterId, Guid.CreateVersion7());
        _guilds.GetMembershipAsync(GuildId, CharacterId, Arg.Any<CancellationToken>()).Returns(membership);

        var outcome = await Handler.HandleAsync(new SetMemberNoteCommand(PlayerId, GuildId, CharacterId, "Tres bon joueur"));

        Assert.Equal(SetMemberNoteOutcome.Updated, outcome);
        Assert.Equal("Tres bon joueur", membership.Note);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenTheNoteIsNull_ClearsIt()
    {
        GrantPermission();
        var membership = new GuildMembership(GuildId, CharacterId, Guid.CreateVersion7()) { Note = "ancienne" };
        _guilds.GetMembershipAsync(GuildId, CharacterId, Arg.Any<CancellationToken>()).Returns(membership);

        await Handler.HandleAsync(new SetMemberNoteCommand(PlayerId, GuildId, CharacterId, null));

        Assert.Null(membership.Note);
    }
}

public class KickMemberCommandHandlerTests
{
    private static readonly Guid PlayerId = Guid.CreateVersion7();
    private static readonly Guid GuildId = Guid.CreateVersion7();
    private static readonly Guid MemberRankId = Guid.CreateVersion7();

    private readonly IPlayerRepository _players = Substitute.For<IPlayerRepository>();
    private readonly IGuildRepository _guilds = Substitute.For<IGuildRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private KickMemberCommandHandler Handler => new(_players, _guilds, _unitOfWork);

    private readonly Character _character =
        new(PlayerId, Guid.CreateVersion7(), Guid.CreateVersion7(), "Sylvane", "Hyjal", 80);

    private GuildMembership MembershipExists(Guid rankId)
    {
        var membership = new GuildMembership(GuildId, _character.Id, rankId);
        _guilds.GetMembershipAsync(GuildId, _character.Id, Arg.Any<CancellationToken>()).Returns(membership);
        return membership;
    }

    private void RankIs(Guid rankId, bool isLeader)
        => _guilds.GetRankAsync(GuildId, rankId, Arg.Any<CancellationToken>())
                  .Returns(new GuildRank(GuildId, isLeader ? "Chef de guilde" : "Membre", 0, [], isLeader));

    [Fact]
    public async Task WhenCharacterIsNotAMember_ReturnsMembershipNotFound()
    {
        var outcome = await Handler.HandleAsync(new KickMemberCommand(PlayerId, GuildId, _character.Id));

        Assert.Equal(KickMemberOutcome.MembershipNotFound, outcome);
    }

    [Fact]
    public async Task WhenCallerNeitherOwnsTheCharacterNorMayKick_ReturnsForbidden()
    {
        MembershipExists(MemberRankId);

        var outcome = await Handler.HandleAsync(new KickMemberCommand(Guid.CreateVersion7(), GuildId, _character.Id));

        Assert.Equal(KickMemberOutcome.Forbidden, outcome);
        _guilds.DidNotReceive().RemoveMembership(Arg.Any<GuildMembership>());
    }

    /// <summary>Premier acteur : le membre quitte de lui-meme.</summary>
    [Fact]
    public async Task WhenCallerOwnsTheCharacter_LeavesWithoutCheckingPermissions()
    {
        var membership = MembershipExists(MemberRankId);
        RankIs(MemberRankId, isLeader: false);
        _players.GetCharacterAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(_character);

        var outcome = await Handler.HandleAsync(new KickMemberCommand(PlayerId, GuildId, _character.Id));

        Assert.Equal(KickMemberOutcome.Kicked, outcome);
        _guilds.Received(1).RemoveMembership(membership);
        await _guilds.DidNotReceive().HasPermissionAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<GuildPermission>(), Arg.Any<CancellationToken>());
    }

    /// <summary>Second acteur : un grade exclut le membre.</summary>
    [Fact]
    public async Task WhenCallerMayKick_RemovesTheMembership()
    {
        var membership = MembershipExists(MemberRankId);
        RankIs(MemberRankId, isLeader: false);
        Guid officerId = Guid.CreateVersion7();
        _guilds.HasPermissionAsync(GuildId, officerId, GuildPermission.KickMember, Arg.Any<CancellationToken>()).Returns(true);

        var outcome = await Handler.HandleAsync(new KickMemberCommand(officerId, GuildId, _character.Id));

        Assert.Equal(KickMemberOutcome.Kicked, outcome);
        _guilds.Received(1).RemoveMembership(membership);
    }

    /// <summary>Le chef ne peut ni partir ni etre exclu : la guilde resterait sans chef.</summary>
    [Fact]
    public async Task WhenTheTargetIsTheLeader_ReturnsCannotKickLeader()
    {
        Guid leaderRankId = Guid.CreateVersion7();
        MembershipExists(leaderRankId);
        RankIs(leaderRankId, isLeader: true);
        _players.GetCharacterAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(_character);

        var outcome = await Handler.HandleAsync(new KickMemberCommand(PlayerId, GuildId, _character.Id));

        Assert.Equal(KickMemberOutcome.CannotKickLeader, outcome);
        _guilds.DidNotReceive().RemoveMembership(Arg.Any<GuildMembership>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
