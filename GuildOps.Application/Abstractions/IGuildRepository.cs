using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Abstractions;

public interface IGuildRepository
{
    Task<bool> NameExistsOnServerAsync(string server, string name, CancellationToken cancellationToken = default);

    Task<bool> CharacterHasMembershipAsync(Guid characterId, CancellationToken cancellationToken = default);

    Task<Guild?> GetAsync(Guid guildId, CancellationToken cancellationToken = default);

    Task<Guild?> GetWithMembersAsync(Guid guildId, CancellationToken cancellationToken = default);

    Task<bool> HasPermissionAsync(Guid guildId, Guid playerId, GuildPermission permission, CancellationToken cancellationToken = default);

    Task<Guid?> GetDefaultRankIdAsync(Guid guildId, CancellationToken cancellationToken = default);

    Task<bool> ApplicationExistsAsync(Guid guildId, Guid characterId, CancellationToken cancellationToken = default);

    Task<GuildApplication?> GetApplicationAsync(Guid guildId, Guid characterId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GuildApplication>> GetApplicationsAsync(Guid guildId, CancellationToken cancellationToken = default);

    void Add(Guild guild);

    void AddApplication(GuildApplication application);

    void RemoveApplication(GuildApplication application);

    void AddMembership(GuildMembership membership);

    Task<bool> InvitationExistsAsync(Guid guildId, Guid characterId, CancellationToken cancellationToken = default);

    Task<GuildInvitation?> GetInvitationAsync(Guid guildId, Guid characterId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GuildInvitation>> GetInvitationsAsync(Guid guildId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GuildInvitation>> GetInvitationsForPlayerAsync(Guid playerId, CancellationToken cancellationToken = default);

    void AddInvitation(GuildInvitation invitation);

    void RemoveInvitation(GuildInvitation invitation);
}
