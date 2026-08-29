using GuildOps.Domain.Guilds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuildOps.Infrastructure.Persistence.Configurations;

internal sealed class GuildRankConfiguration : IEntityTypeConfiguration<GuildRank>
{
    public void Configure(EntityTypeBuilder<GuildRank> builder)
    {
        builder.ToTable("GuildRanks");

        builder.HasOne(rank => rank.Guild)
            .WithMany(guild => guild.Ranks)
            .HasForeignKey(rank => rank.GuildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rank => new { rank.GuildId, rank.Name })
            .IsUnique();

        builder.HasIndex(rank => new { rank.GuildId, rank.SortOrder })
            .IsUnique();

        builder.HasIndex(rank => rank.GuildId, "IX_GuildRanks_GuildId_Leader")
            .IsUnique()
            .HasFilter("[IsLeader] = 1");

        builder.HasIndex(rank => rank.GuildId, "IX_GuildRanks_GuildId_Default")
            .IsUnique()
            .HasFilter("[IsDefault] = 1");
    }
}
