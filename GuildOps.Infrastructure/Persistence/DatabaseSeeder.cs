using GuildOps.Domain.Games;
using Microsoft.EntityFrameworkCore;

namespace GuildOps.Infrastructure.Persistence;

internal static class DatabaseSeeder
{
    private static readonly (string Name, int MaxLevel, string[] Classes, string[] Roles)[] Catalogue =
    [
        ("World of Warcraft", 80,
        [
            "Guerrier", "Paladin", "Chasseur", "Voleur", "Prêtre",
            "Chevalier de la mort", "Chaman", "Mage", "Démoniste",
            "Moine", "Druide", "Chasseur de démons", "Évocateur"
        ],
        ["Tank", "Soigneur", "DPS"])
    ];

    public static void Seed(ApplicationDbContext context)
    {
        if (context.Games.Any())
        {
            return;
        }

        Build(context);
        context.SaveChanges();
    }

    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Games.AnyAsync(cancellationToken))
        {
            return;
        }

        Build(context);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static void Build(ApplicationDbContext context)
    {
        foreach ((string name, int maxLevel, string[] classes, string[] roles) in Catalogue)
        {
            var game = new Game(name, maxLevel);

            for (int index = 0; index < classes.Length; index++)
            {
                game.Classes.Add(new CharacterClass(game.Id, classes[index], index + 1));
            }

            for (int index = 0; index < roles.Length; index++)
            {
                game.Roles.Add(new GameRole(game.Id, roles[index], index + 1));
            }

            context.Games.Add(game);
        }
    }
}
