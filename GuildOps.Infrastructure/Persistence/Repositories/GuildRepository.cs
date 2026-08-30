using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;
using Microsoft.EntityFrameworkCore;

namespace GuildOps.Infrastructure.Persistence.Repositories;

internal sealed class GuildRepository(ApplicationDbContext context) : IGuildRepository
{
    public Task<bool> NameExistsOnServerAsync(string server, string name, CancellationToken cancellationToken = default)
        => context.Guilds
            .AnyAsync(guild => guild.Server == server && guild.Name == name, cancellationToken);

    public Task<bool> CharacterHasMembershipAsync(Guid characterId, CancellationToken cancellationToken = default)
        => context.Set<GuildMembership>()
            .AnyAsync(membership => membership.CharacterId == characterId, cancellationToken);

    public Task<Guild?> GetAsync(Guid guildId, CancellationToken cancellationToken = default)
        => context.Guilds
            .AsNoTracking()
            .FirstOrDefaultAsync(guild => guild.Id == guildId, cancellationToken);

    public Task<Guild?> GetWithMembersAsync(Guid guildId, CancellationToken cancellationToken = default)
        => context.Guilds
            .AsNoTracking()
            .Include(guild => guild.Ranks)
            .Include(guild => guild.Memberships).ThenInclude(membership => membership.Character)
            .Include(guild => guild.Memberships).ThenInclude(membership => membership.Rank)
            .FirstOrDefaultAsync(guild => guild.Id == guildId, cancellationToken);

    public async Task<bool> HasPermissionAsync(Guid guildId, Guid playerId, GuildPermission permission, CancellationToken cancellationToken = default)
    {
        List<GuildPermission>? permissions = await context.Set<GuildMembership>()
            .AsNoTracking()
            .Where(membership => membership.GuildId == guildId && membership.Character!.PlayerId == playerId)
            .Select(membership => membership.Rank!.Permissions)
            .FirstOrDefaultAsync(cancellationToken);

        return permissions is not null && permissions.Contains(permission);
    }

    public async Task<Guid?> GetDefaultRankIdAsync(Guid guildId, CancellationToken cancellationToken = default)
    {
        var rank = await context.Set<GuildRank>()
            .AsNoTracking()
            .Where(entry => entry.GuildId == guildId && entry.IsDefault)
            .Select(entry => new { entry.Id })
            .FirstOrDefaultAsync(cancellationToken);

        return rank?.Id;
    }

    public Task<bool> ApplicationExistsAsync(Guid guildId, Guid characterId, CancellationToken cancellationToken = default)
        => context.Set<GuildApplication>()
            .AnyAsync(application => application.GuildId == guildId && application.CharacterId == characterId, cancellationToken);

    public Task<GuildApplication?> GetApplicationAsync(Guid guildId, Guid characterId, CancellationToken cancellationToken = default)
        => context.Set<GuildApplication>()
            .FirstOrDefaultAsync(application => application.GuildId == guildId && application.CharacterId == characterId, cancellationToken);

    public async Task<IReadOnlyList<GuildApplication>> GetApplicationsAsync(Guid guildId, CancellationToken cancellationToken = default)
        => await context.Set<GuildApplication>()
            .AsNoTracking()
            .Include(application => application.Character)
            .Where(application => application.GuildId == guildId)
            .OrderBy(application => application.CreatedAt)
            .ToListAsync(cancellationToken);

    public void Add(Guild guild) => context.Guilds.Add(guild);

    public void AddApplication(GuildApplication application) => context.Set<GuildApplication>().Add(application);

    public void RemoveApplication(GuildApplication application) => context.Set<GuildApplication>().Remove(application);

    public void AddMembership(GuildMembership membership) => context.Set<GuildMembership>().Add(membership);
}
