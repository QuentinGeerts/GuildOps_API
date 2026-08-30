using GuildOps.Application.Abstractions;
using GuildOps.Application.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Players;

public class LoginCommandHandlerTests
{
    private readonly IPlayerCredentialStore _credentials = Substitute.For<IPlayerCredentialStore>();
    private readonly ITokenGenerator _tokens = Substitute.For<ITokenGenerator>();
    private readonly IRefreshTokenStore _refreshTokens = Substitute.For<IRefreshTokenStore>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private LoginCommandHandler Handler => new(_credentials, _tokens, _refreshTokens, _unitOfWork);

    private static readonly LoginCommand Command = new("quentin@example.com", "MonMotDePasse!42");

    [Fact]
    public async Task WhenCredentialsDoNotMatch_ReturnsInvalidCredentials()
    {
        var result = await Handler.HandleAsync(Command);

        Assert.Equal(LoginOutcome.InvalidCredentials, result.Outcome);
        Assert.Null(result.Tokens);
        _tokens.DidNotReceive().Generate(Arg.Any<Guid>());
        _refreshTokens.DidNotReceive().Create(Arg.Any<Guid>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenCredentialsMatch_ReturnsBothTokensAndSaves()
    {
        Guid playerId = Guid.CreateVersion7();
        _credentials.FindPlayerIdAsync("quentin@example.com", "MonMotDePasse!42", Arg.Any<CancellationToken>()).Returns(playerId);
        _tokens.Generate(playerId).Returns(new AccessToken("acces", DateTimeOffset.UtcNow.AddHours(1)));
        _refreshTokens.Create(playerId).Returns(new IssuedRefreshToken("rafraichissement", DateTimeOffset.UtcNow.AddDays(14)));

        var result = await Handler.HandleAsync(Command);

        Assert.Equal(LoginOutcome.Succeeded, result.Outcome);
        Assert.Equal("acces", result.Tokens!.AccessToken);
        Assert.Equal("rafraichissement", result.Tokens.RefreshToken);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
