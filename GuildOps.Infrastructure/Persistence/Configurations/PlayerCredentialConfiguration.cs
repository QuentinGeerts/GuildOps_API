using GuildOps.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuildOps.Infrastructure.Persistence.Configurations;

internal sealed class PlayerCredentialConfiguration : IEntityTypeConfiguration<PlayerCredential>
{
    public void Configure(EntityTypeBuilder<PlayerCredential> builder)
    {
        builder.ToTable("PlayerCredentials");

        builder.HasOne(credential => credential.Player)
            .WithOne()
            .HasForeignKey<PlayerCredential>(credential => credential.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(credential => credential.Email)
            .IsUnique();
    }
}
