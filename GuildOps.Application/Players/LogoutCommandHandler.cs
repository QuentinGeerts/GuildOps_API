using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Players;

internal sealed class LogoutCommandHandler(IRefreshTokenStore refreshTokens, IUnitOfWork unitOfWork)
    : ICommandHandler<LogoutCommand>
{
    public async Task HandleAsync(LogoutCommand command, CancellationToken cancellationToken = default)
    {
        // revoquer un jeton inconnu est sans effet : on ne revele pas s'il existait
        await refreshTokens.RevokeAsync(command.RefreshToken, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
