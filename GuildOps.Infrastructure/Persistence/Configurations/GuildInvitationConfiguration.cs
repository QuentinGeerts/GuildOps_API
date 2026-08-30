using GuildOps.Domain.Guilds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuildOps.Infrastructure.Persistence.Configurations;

internal sealed class GuildInvitationConfiguration : IEntityTypeConfiguration<GuildInvitation>
{
    public void Configure(EntityTypeBuilder<GuildInvitation> builder)
    {
        builder.ToTable("GuildInvitations");

        builder.HasOne(invitation => invitation.Guild)
            .WithMany()
            .HasForeignKey(invitation => invitation.GuildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(invitation => invitation.Character)
            .WithMany()
            .HasForeignKey(invitation => invitation.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(invitation => new { invitation.GuildId, invitation.CharacterId })
            .IsUnique();

        builder.Property(invitation => invitation.Message)
            .HasMaxLength(1000);
    }
}
