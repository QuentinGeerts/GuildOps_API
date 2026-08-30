using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Players;

internal sealed class RefreshTokensCommandHandler(
    ITokenGenerator tokenGenerator,
    IRefreshTokenStore refreshTokens,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefreshTokensCommand, RefreshTokensResult>
{
    public async Task<RefreshTokensResult> HandleAsync(RefreshTokensCommand command, CancellationToken cancellationToken = default)
    {
        Guid? playerId = await refreshTokens.ConsumeAsync(command.RefreshToken, cancellationToken);

        if (playerId is null)
        {
            return RefreshTokensResult.InvalidToken;
        }

        var access = tokenGenerator.Generate(playerId.Value);
        var refresh = refreshTokens.Create(playerId.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return RefreshTokensResult.Refreshed(AuthTokensDto.From(access, refresh));
    }
}
