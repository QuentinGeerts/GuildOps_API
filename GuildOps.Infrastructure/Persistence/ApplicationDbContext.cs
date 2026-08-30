using GuildOps.Application.Abstractions;
using GuildOps.Domain.Games;
using GuildOps.Domain.Guilds;
using GuildOps.Domain.Players;
using Microsoft.EntityFrameworkCore;

namespace GuildOps.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Guild> Guilds => Set<Guild>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>().HaveMaxLength(256);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (UniqueConstraintReader.TryRead(exception, out string? constraintName))
        {
            throw new UniqueConstraintException(constraintName, exception);
        }
    }

}
