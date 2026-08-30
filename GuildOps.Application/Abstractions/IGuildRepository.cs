using GuildOps.Application.Guilds;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Abstractions;

public interface IGuildRepository
{
    Task<bool> NameExistsOnServerAsync(string server, string name, CancellationToken cancellationToken = default);

    Task<bool> CharacterHasMembershipAsync(Guid characterId, CancellationToken cancellationToken = default);

    Task<Guild?> GetAsync(Guid guildId, CancellationToken cancellationToken = default);

    Task<Guild?> GetForUpdateAsync(Guid guildId, CancellationToken cancellationToken = default);

    Task<Guild?> GetWithMembersAsync(Guid guildId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GuildSummaryDto>> SearchAsync(Guid? gameId, string? server, string? name, CancellationToken cancellationToken = default);

    Task<bool> HasPermissionAsync(Guid guildId, Guid playerId, GuildPermission permission, CancellationToken cancellationToken = default);

    Task<Guid?> GetDefaultRankIdAsync(Guid guildId, CancellationToken cancellationToken = default);

    Task<GuildRank?> GetRankAsync(Guid guildId, Guid rankId, CancellationToken cancellationToken = default);

    Task<GuildMembership?> GetMembershipAsync(Guid guildId, Guid characterId, CancellationToken cancellationToken = default);

    Task<GuildMembership?> GetLeaderMembershipAsync(Guid guildId, CancellationToken cancellationToken = default);

    Task<bool> ApplicationExistsAsync(Guid guildId, Guid characterId, CancellationToken cancellationToken = default);

    Task<GuildApplication?> GetApplicationAsync(Guid guildId, Guid characterId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GuildApplication>> GetApplicationsAsync(Guid guildId, CancellationToken cancellationToken = default);

    Task<bool> InvitationExistsAsync(Guid guildId, Guid characterId, CancellationToken cancellationToken = default);

    Task<GuildInvitation?> GetInvitationAsync(Guid guildId, Guid characterId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GuildInvitation>> GetInvitationsAsync(Guid guildId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GuildInvitation>> GetInvitationsForPlayerAsync(Guid playerId, CancellationToken cancellationToken = default);

    void Add(Guild guild);

    void AddApplication(GuildApplication application);

    void RemoveApplication(GuildApplication application);

    void AddInvitation(GuildInvitation invitation);

    void RemoveInvitation(GuildInvitation invitation);

    void AddMembership(GuildMembership membership);

    void RemoveMembership(GuildMembership membership);

    Task<Guild?> GetGuildLedByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guild>> GetGuildsLedByPlayerAsync(Guid playerId, CancellationToken cancellationToken = default);

    void RemoveGuild(Guild guild);
}
