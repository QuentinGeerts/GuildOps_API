using GuildOps.Application.Abstractions;

namespace GuildOps.Application.Players;

internal sealed class LoginCommandHandler(IPlayerCredentialStore credentials, ITokenGenerator tokenGenerator)
    : ICommandHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        Guid? playerId = await credentials.FindPlayerIdAsync(command.Email, command.Password, cancellationToken);

        return playerId is null
            ? LoginResult.InvalidCredentials
            : LoginResult.Succeeded(tokenGenerator.Generate(playerId.Value));
    }
}
