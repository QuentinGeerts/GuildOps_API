namespace GuildOps.Application.Abstractions;

/// <summary>Valide en une seule transaction toutes les modifications accumulées par les repositories.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
