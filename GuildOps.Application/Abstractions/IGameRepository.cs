using GuildOps.Domain.Games;

namespace GuildOps.Application.Abstractions;

public interface IGameRepository
{
    Task<IReadOnlyList<Game>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Game?> GetWithClassesAndRolesAsync(Guid id, CancellationToken cancellationToken = default);
}
