using GuildOps.Application.Abstractions;
using GuildOps.Domain.Players;
using Microsoft.EntityFrameworkCore;

namespace GuildOps.Infrastructure.Persistence.Repositories;

internal sealed class PlayerRepository(ApplicationDbContext context) : IPlayerRepository
{
    public Task<bool> AccountNameExistsAsync(string accountName, CancellationToken cancellationToken = default)
        => context.Players
            .AnyAsync(player => player.AccountName == accountName, cancellationToken);

    public Task<bool> ExistsAsync(Guid playerId, CancellationToken cancellationToken = default)
        => context.Players
            .AnyAsync(player => player.Id == playerId, cancellationToken);

    public Task<bool> CharacterNameExistsAsync(string server, string name, CancellationToken cancellationToken = default)
        => context.Set<Character>()
            .AnyAsync(character => character.Server == server && character.Name == name, cancellationToken);

    public Task<Character?> GetCharacterAsync(Guid characterId, CancellationToken cancellationToken = default)
        => context.Set<Character>()
            .AsNoTracking()
            .FirstOrDefaultAsync(character => character.Id == characterId, cancellationToken);

    public Task<Player?> GetWithCharactersAsync(Guid playerId, CancellationToken cancellationToken = default)
        => context.Players
            .AsNoTracking()
            .Include(player => player.Characters)
            .FirstOrDefaultAsync(player => player.Id == playerId, cancellationToken);

    public void Add(Player player) => context.Players.Add(player);

    public void AddCharacter(Character character) => context.Set<Character>().Add(character);
}
