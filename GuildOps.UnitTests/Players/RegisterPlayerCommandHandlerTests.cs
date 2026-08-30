using GuildOps.Application.Abstractions;
using GuildOps.Application.Players;
using GuildOps.Domain.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Players;

public class RegisterPlayerCommandHandlerTests
{
    private readonly IPlayerRepository _players = Substitute.For<IPlayerRepository>();
    private readonly IPlayerCredentialStore _credentials = Substitute.For<IPlayerCredentialStore>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private RegisterPlayerCommandHandler Handler => new(_players, _credentials, _unitOfWork);

    private static readonly RegisterPlayerCommand Command = new("quentin", "quentin@example.com", "MonMotDePasse!42");

    [Fact]
    public async Task WhenAccountNameIsTaken_ReturnsAccountNameTaken()
    {
        _players.AccountNameExistsAsync("quentin", Arg.Any<CancellationToken>()).Returns(true);

        var result = await Handler.HandleAsync(Command);

        Assert.Equal(RegisterPlayerOutcome.AccountNameTaken, result.Outcome);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenEmailIsTaken_ReturnsEmailTaken()
    {
        _credentials.EmailExistsAsync("quentin@example.com", Arg.Any<CancellationToken>()).Returns(true);

        var result = await Handler.HandleAsync(Command);

        Assert.Equal(RegisterPlayerOutcome.EmailTaken, result.Outcome);
    }

    [Fact]
    public async Task WhenTheAccountNameIndexIsViolated_ReturnsAccountNameTaken()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
                   .Returns<Task<int>>(_ => throw new UniqueConstraintException("IX_Players_AccountName", new Exception()));

        var result = await Handler.HandleAsync(Command);

        Assert.Equal(RegisterPlayerOutcome.AccountNameTaken, result.Outcome);
    }

    [Fact]
    public async Task WhenTheEmailIndexIsViolated_ReturnsEmailTaken()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
                   .Returns<Task<int>>(_ => throw new UniqueConstraintException("IX_PlayerCredentials_Email", new Exception()));

        var result = await Handler.HandleAsync(Command);

        Assert.Equal(RegisterPlayerOutcome.EmailTaken, result.Outcome);
    }

    [Fact]
    public async Task WhenAnotherConstraintIsViolated_TheExceptionEscapes()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
                   .Returns<Task<int>>(_ => throw new UniqueConstraintException("IX_Autre_Chose", new Exception()));

        await Assert.ThrowsAsync<UniqueConstraintException>(() => Handler.HandleAsync(Command));
    }

    [Fact]
    public async Task WhenEverythingIsValid_CreatesThePlayerAndItsCredentials()
    {
        var result = await Handler.HandleAsync(Command);

        Assert.Equal(RegisterPlayerOutcome.Created, result.Outcome);
        Assert.Equal("quentin", result.Player!.AccountName);
        _players.Received(1).Add(Arg.Is<Player>(player => player.AccountName == "quentin"));
        _credentials.Received(1).Create(Arg.Any<Guid>(), "quentin@example.com", "MonMotDePasse!42");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
