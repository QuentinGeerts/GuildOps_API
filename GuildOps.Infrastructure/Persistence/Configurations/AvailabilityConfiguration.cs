using GuildOps.Domain.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuildOps.Infrastructure.Persistence.Configurations;

internal sealed class AvailabilityConfiguration : IEntityTypeConfiguration<Availability>
{
    public void Configure(EntityTypeBuilder<Availability> builder)
    {
        builder.ToTable("Availabilities");

        builder.HasOne(availability => availability.Character)
            .WithMany(character => character.Availabilities)
            .HasForeignKey(availability => availability.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(availability => new { availability.CharacterId, availability.Day, availability.Slot })
            .IsUnique();
    }
}
