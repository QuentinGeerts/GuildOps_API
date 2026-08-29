using GuildOps.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuildOps.Infrastructure.Persistence.Configurations;

internal sealed class CharacterClassConfiguration : IEntityTypeConfiguration<CharacterClass>
{
    public void Configure(EntityTypeBuilder<CharacterClass> builder)
    {
        builder.ToTable("CharacterClasses");

        builder.HasOne(characterClass => characterClass.Game)
            .WithMany(game => game.Classes)
            .HasForeignKey(characterClass => characterClass.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(characterClass => new { characterClass.GameId, characterClass.Name })
            .IsUnique();
    }
}
