using GuildOps.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuildOps.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasOne(token => token.Player)
            .WithMany()
            .HasForeignKey(token => token.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(token => token.TokenHash)
            .IsUnique();
    }
}
