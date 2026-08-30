using GuildOps.Domain.Guilds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuildOps.Infrastructure.Persistence.Configurations;

internal sealed class GuildApplicationConfiguration : IEntityTypeConfiguration<GuildApplication>
{
    public void Configure(EntityTypeBuilder<GuildApplication> builder)
    {
        builder.ToTable("GuildApplications");

        builder.HasOne(application => application.Guild)
            .WithMany()
            .HasForeignKey(application => application.GuildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(application => application.Character)
            .WithMany()
            .HasForeignKey(application => application.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(application => new { application.GuildId, application.CharacterId })
            .IsUnique();

        builder.Property(application => application.Message)
            .HasMaxLength(1000);
    }
}
