using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Abstractions;

public interface IGuildRepository
{
    Task<bool> NameExistsOnServerAsync(string server, string name, CancellationToken cancellationToken = default);

    Task<bool> CharacterHasMembershipAsync(Guid characterId, CancellationToken cancellationToken = default);

    Task<Guild?> GetWithMembersAsync(Guid guildId, CancellationToken cancellationToken = default);

    void Add(Guild guild);
}
