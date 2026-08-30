using GuildOps.Application.Abstractions;
using GuildOps.Application.Players;
using NSubstitute;

namespace GuildOps.UnitTests.Players;

public class LogoutCommandHandlerTests
{
    private readonly IRefreshTokenStore _refreshTokens = Substitute.For<IRefreshTokenStore>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private LogoutCommandHandler Handler => new(_refreshTokens, _unitOfWork);

    /// <summary>Revoquer un jeton inconnu reste sans effet visible : on ne revele pas s'il existait.</summary>
    [Fact]
    public async Task WhenTokenIsUnknown_StillSucceeds()
    {
        await Handler.HandleAsync(new LogoutCommand("inconnu"));

        await _refreshTokens.Received(1).RevokeAsync("inconnu", Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
