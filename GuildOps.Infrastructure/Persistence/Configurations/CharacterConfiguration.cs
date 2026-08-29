using GuildOps.Domain.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuildOps.Infrastructure.Persistence.Configurations;

internal sealed class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.ToTable("Characters", table =>
            table.HasCheckConstraint("CK_Characters_Level", "[Level] >= 1"));

        builder.HasOne(character => character.Player)
            .WithMany(player => player.Characters)
            .HasForeignKey(character => character.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(character => character.Game)
            .WithMany()
            .HasForeignKey(character => character.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(character => character.CharacterClass)
            .WithMany()
            .HasForeignKey(character => character.CharacterClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(character => new { character.Server, character.Name })
            .IsUnique();
    }
}
