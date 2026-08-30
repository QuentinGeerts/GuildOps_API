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

    public Task<Character?> GetCharacterWithDetailsAsync(Guid characterId, CancellationToken cancellationToken = default)
        => context.Set<Character>()
            .AsNoTracking()
            .Include(character => character.Roles).ThenInclude(assignment => assignment.GameRole)
            .Include(character => character.Availabilities)
            .FirstOrDefaultAsync(character => character.Id == characterId, cancellationToken);

    public Task<Character?> GetCharacterForUpdateAsync(Guid characterId, CancellationToken cancellationToken = default)
        => context.Set<Character>()
            .Include(character => character.Roles)
            .Include(character => character.Availabilities)
            .FirstOrDefaultAsync(character => character.Id == characterId, cancellationToken);

    public void AddCharacterRole(CharacterGameRole assignment) => context.Set<CharacterGameRole>().Add(assignment);

    public void RemoveCharacterRole(CharacterGameRole assignment) => context.Set<CharacterGameRole>().Remove(assignment);

    public void AddAvailability(Availability availability) => context.Set<Availability>().Add(availability);

    public void RemoveAvailability(Availability availability) => context.Set<Availability>().Remove(availability);

    public Task<Player?> GetForUpdateAsync(Guid playerId, CancellationToken cancellationToken = default)
        => context.Players
            .FirstOrDefaultAsync(player => player.Id == playerId, cancellationToken);

    public void Remove(Player player) => context.Players.Remove(player);

    public void RemoveCharacter(Character character) => context.Set<Character>().Remove(character);
}
