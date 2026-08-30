using GuildOps.Domain.Games;
using Microsoft.EntityFrameworkCore;

namespace GuildOps.Infrastructure.Persistence;

internal static class DatabaseSeeder
{
    private static readonly (string Name, int MaxLevel, string[] Classes)[] Catalogue =
    [
        ("World of Warcraft", 80,
        [
            "Guerrier", "Paladin", "Chasseur", "Voleur", "Prêtre",
            "Chevalier de la mort", "Chaman", "Mage", "Démoniste",
            "Moine", "Druide", "Chasseur de démons", "Évocateur"
        ])
    ];

    public static void Seed(ApplicationDbContext context)
    {
        if (!Prepare(context))
        {
            return;
        }

        context.SaveChanges();
    }

    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (!await PrepareAsync(context, cancellationToken))
        {
            return;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static bool Prepare(ApplicationDbContext context)
        => context.Games.Any() ? false : Build(context);

    private static async Task<bool> PrepareAsync(ApplicationDbContext context, CancellationToken cancellationToken)
        => await context.Games.AnyAsync(cancellationToken) ? false : Build(context);

    private static bool Build(ApplicationDbContext context)
    {
        foreach ((string name, int maxLevel, string[] classes) in Catalogue)
        {
            var game = new Game(name, maxLevel);

            for (int index = 0; index < classes.Length; index++)
            {
                game.Classes.Add(new CharacterClass(game.Id, classes[index], index + 1));
            }

            context.Games.Add(game);
        }

        return true;
    }
}
