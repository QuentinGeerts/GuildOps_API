using GuildOps.Application.Abstractions;
using GuildOps.Application.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Players;

public class RefreshTokensCommandHandlerTests
{
    private readonly ITokenGenerator _tokens = Substitute.For<ITokenGenerator>();
    private readonly IRefreshTokenStore _refreshTokens = Substitute.For<IRefreshTokenStore>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private RefreshTokensCommandHandler Handler => new(_tokens, _refreshTokens, _unitOfWork);

    [Fact]
    public async Task WhenTokenIsUnknownOrExpired_ReturnsInvalidToken()
    {
        var result = await Handler.HandleAsync(new RefreshTokensCommand("inconnu"));

        Assert.Equal(RefreshTokensOutcome.InvalidToken, result.Outcome);
        Assert.Null(result.Tokens);
        _tokens.DidNotReceive().Generate(Arg.Any<Guid>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>La rotation est le coeur du dispositif : un rafraichissement emet un nouveau couple.</summary>
    [Fact]
    public async Task WhenTokenIsValid_IssuesANewPairAndSaves()
    {
        Guid playerId = Guid.CreateVersion7();
        _refreshTokens.ConsumeAsync("ancien", Arg.Any<CancellationToken>()).Returns(playerId);
        _tokens.Generate(playerId).Returns(new AccessToken("nouvel-acces", DateTimeOffset.UtcNow.AddHours(1)));
        _refreshTokens.Create(playerId).Returns(new IssuedRefreshToken("nouveau", DateTimeOffset.UtcNow.AddDays(14)));

        var result = await Handler.HandleAsync(new RefreshTokensCommand("ancien"));

        Assert.Equal(RefreshTokensOutcome.Refreshed, result.Outcome);
        Assert.Equal("nouvel-acces", result.Tokens!.AccessToken);
        Assert.Equal("nouveau", result.Tokens.RefreshToken);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
