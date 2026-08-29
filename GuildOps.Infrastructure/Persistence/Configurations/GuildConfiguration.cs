using GuildOps.Domain.Guilds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuildOps.Infrastructure.Persistence.Configurations;

internal sealed class GuildConfiguration : IEntityTypeConfiguration<Guild>
{
    public void Configure(EntityTypeBuilder<Guild> builder)
    {
        builder.ToTable("Guilds");

        builder.HasOne(guild => guild.Game)
            .WithMany()
            .HasForeignKey(guild => guild.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(guild => new { guild.Server, guild.Name })
            .IsUnique();

        builder.Property(guild => guild.Description)
            .HasMaxLength(2000);

        builder.Property(guild => guild.ChatUrl)
            .HasMaxLength(512);
    }
}
