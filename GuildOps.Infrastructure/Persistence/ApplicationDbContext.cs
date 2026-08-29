using Microsoft.EntityFrameworkCore;

namespace GuildOps.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        configurationBuilder.Properties<string>().HaveMaxLength(256);
        configurationBuilder.Properties<DateTime>().HaveColumnType("datetime2");
        configurationBuilder.Properties<DateTimeOffset>().HaveColumnType("datetimeoffset");
    }
}
