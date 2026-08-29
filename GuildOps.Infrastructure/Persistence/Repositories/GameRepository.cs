using GuildOps.Application.Abstractions;
using GuildOps.Domain.Games;
using Microsoft.EntityFrameworkCore;

namespace GuildOps.Infrastructure.Persistence.Repositories;

internal sealed class GameRepository(ApplicationDbContext context) : IGameRepository
{
    public async Task<IReadOnlyList<Game>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Games
            .AsNoTracking()
            .OrderBy(game => game.Name)
            .ToListAsync(cancellationToken);

    public Task<Game?> GetWithClassesAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Games
            .AsNoTracking()
            .Include(game => game.Classes
                .OrderBy(characterClass => characterClass.SortOrder)
                .ThenBy(characterClass => characterClass.Name))
            .FirstOrDefaultAsync(game => game.Id == id, cancellationToken);
}
