using GuildOps.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuildOps.Infrastructure.Persistence.Configurations;

internal sealed class GameRoleConfiguration : IEntityTypeConfiguration<GameRole>
{
    public void Configure(EntityTypeBuilder<GameRole> builder)
    {
        builder.ToTable("GameRoles");

        builder.HasOne(role => role.Game)
            .WithMany(game => game.Roles)
            .HasForeignKey(role => role.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(role => new { role.GameId, role.Name })
            .IsUnique();
    }
}
