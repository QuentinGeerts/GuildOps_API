using GuildOps.Domain.Players;

namespace GuildOps.Application.Abstractions;

public interface IPlayerRepository
{
    Task<bool> AccountNameExistsAsync(string accountName, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid playerId, CancellationToken cancellationToken = default);

    Task<bool> CharacterNameExistsAsync(string server, string name, CancellationToken cancellationToken = default);

    Task<Character?> GetCharacterAsync(Guid characterId, CancellationToken cancellationToken = default);

    Task<Character?> GetCharacterWithDetailsAsync(Guid characterId, CancellationToken cancellationToken = default);

    Task<Character?> GetCharacterForUpdateAsync(Guid characterId, CancellationToken cancellationToken = default);

    Task<Player?> GetWithCharactersAsync(Guid playerId, CancellationToken cancellationToken = default);

    void Add(Player player);

    void AddCharacter(Character character);

    void AddCharacterRole(CharacterGameRole assignment);

    void RemoveCharacterRole(CharacterGameRole assignment);

    void AddAvailability(Availability availability);

    void RemoveAvailability(Availability availability);
}
