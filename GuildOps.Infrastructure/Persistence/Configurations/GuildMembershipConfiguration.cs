using GuildOps.Domain.Guilds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuildOps.Infrastructure.Persistence.Configurations;

internal sealed class GuildMembershipConfiguration : IEntityTypeConfiguration<GuildMembership>
{
    public void Configure(EntityTypeBuilder<GuildMembership> builder)
    {
        builder.ToTable("GuildMemberships");

        builder.HasOne(membership => membership.Guild)
            .WithMany(guild => guild.Memberships)
            .HasForeignKey(membership => membership.GuildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(membership => membership.Character)
            .WithOne(character => character.Membership)
            .HasForeignKey<GuildMembership>(membership => membership.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(membership => membership.Rank)
            .WithMany()
            .HasForeignKey(membership => membership.GuildRankId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(membership => membership.Note)
            .HasMaxLength(1000);
    }
}
