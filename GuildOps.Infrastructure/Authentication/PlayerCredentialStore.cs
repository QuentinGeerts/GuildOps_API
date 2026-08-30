using GuildOps.Application.Abstractions;
using GuildOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GuildOps.Infrastructure.Authentication;

internal sealed class PlayerCredentialStore(ApplicationDbContext context, IPasswordHasher passwordHasher)
    : IPlayerCredentialStore
{
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => context.Set<PlayerCredential>()
            .AnyAsync(credential => credential.Email == email, cancellationToken);

    public async Task<Guid?> FindPlayerIdAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        PlayerCredential? credential = await context.Set<PlayerCredential>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entry => entry.Email == email, cancellationToken);

        return passwordHasher.Verify(password, credential?.PasswordHash) ? credential!.PlayerId : null;
    }

    public void Create(Guid playerId, string email, string password)
        => context.Set<PlayerCredential>()
            .Add(new PlayerCredential(playerId, email, passwordHasher.Hash(password)));
}
