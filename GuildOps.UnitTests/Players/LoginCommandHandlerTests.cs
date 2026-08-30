using GuildOps.Application.Abstractions;
using GuildOps.Application.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Players;

public class LoginCommandHandlerTests
{
    private readonly IPlayerCredentialStore _credentials = Substitute.For<IPlayerCredentialStore>();
    private readonly ITokenGenerator _tokens = Substitute.For<ITokenGenerator>();

    private LoginCommandHandler Handler => new(_credentials, _tokens);

    private static readonly LoginCommand Command = new("quentin@example.com", "MonMotDePasse!42");

    [Fact]
    public async Task WhenCredentialsDoNotMatch_ReturnsInvalidCredentials()
    {
        var result = await Handler.HandleAsync(Command);

        Assert.Equal(LoginOutcome.InvalidCredentials, result.Outcome);
        Assert.Null(result.Token);
        _tokens.DidNotReceive().Generate(Arg.Any<Guid>());
    }

    [Fact]
    public async Task WhenCredentialsMatch_ReturnsAToken()
    {
        Guid playerId = Guid.CreateVersion7();
        var token = new AccessToken("jeton", DateTimeOffset.UtcNow.AddHours(1));
        _credentials.FindPlayerIdAsync("quentin@example.com", "MonMotDePasse!42", Arg.Any<CancellationToken>()).Returns(playerId);
        _tokens.Generate(playerId).Returns(token);

        var result = await Handler.HandleAsync(Command);

        Assert.Equal(LoginOutcome.Succeeded, result.Outcome);
        Assert.Equal("jeton", result.Token!.Value);
    }
}
