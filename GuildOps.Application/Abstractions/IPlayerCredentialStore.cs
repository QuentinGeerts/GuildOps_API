namespace GuildOps.Application.Abstractions;

public interface IPlayerCredentialStore
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<Guid?> FindPlayerIdAsync(string email, string password, CancellationToken cancellationToken = default);

    void Create(Guid playerId, string email, string password);
}
