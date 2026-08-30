using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Players;

internal sealed class LoginCommandHandler(
    IPlayerCredentialStore credentials,
    ITokenGenerator tokenGenerator,
    IRefreshTokenStore refreshTokens,
    IUnitOfWork unitOfWork)
    : ICommandHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        Guid? playerId = await credentials.FindPlayerIdAsync(command.Email, command.Password, cancellationToken);

        if (playerId is null)
        {
            return LoginResult.InvalidCredentials;
        }

        var access = tokenGenerator.Generate(playerId.Value);
        var refresh = refreshTokens.Create(playerId.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return LoginResult.Succeeded(AuthTokensDto.From(access, refresh));
    }
}
