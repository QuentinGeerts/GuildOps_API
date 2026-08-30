using GuildOps.Domain.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuildOps.Infrastructure.Persistence.Configurations;

internal sealed class CharacterGameRoleConfiguration : IEntityTypeConfiguration<CharacterGameRole>
{
    public void Configure(EntityTypeBuilder<CharacterGameRole> builder)
    {
        builder.ToTable("CharacterGameRoles");

        builder.HasOne(assignment => assignment.Character)
            .WithMany(character => character.Roles)
            .HasForeignKey(assignment => assignment.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(assignment => assignment.GameRole)
            .WithMany()
            .HasForeignKey(assignment => assignment.GameRoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(assignment => new { assignment.CharacterId, assignment.GameRoleId })
            .IsUnique();
    }
}
