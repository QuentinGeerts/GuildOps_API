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

    public Task<Guild?> GetWithMembersAsync(Guid guildId, CancellationToken cancellationToken = default)
        => context.Guilds
            .AsNoTracking()
            .Include(guild => guild.Ranks)
            .Include(guild => guild.Memberships).ThenInclude(membership => membership.Character)
            .Include(guild => guild.Memberships).ThenInclude(membership => membership.Rank)
            .FirstOrDefaultAsync(guild => guild.Id == guildId, cancellationToken);

    public void Add(Guild guild) => context.Guilds.Add(guild);
}
