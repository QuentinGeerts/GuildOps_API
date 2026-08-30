using GuildOps.Domain.Games;
using Microsoft.EntityFrameworkCore;

namespace GuildOps.Infrastructure.Persistence;

internal static class DatabaseSeeder
{
    private static readonly (string Name, int MaxLevel, string[] Classes, string[] Roles)[] Catalogue =
    [
        ("Dofus", 200,
        [
            "Féca", "Osamodas", "Enutrof", "Sram", "Xélor", "Ecaflip", "Eniripsa",
            "Iop", "Crâ", "Sadida", "Sacrieur", "Pandawa", "Roublard", "Zobal",
            "Steamer", "Eliotrope", "Huppermage", "Ouginak", "Forgelance"
        ],
        ["Tank", "Soigneur", "DPT", "Support"]),

        ("Final Fantasy XIV", 100,
        [
            "Paladin", "Guerrier", "Chevalier noir", "Pistosabreur",
            "Mage blanc", "Érudit", "Astromancien", "Sage",
            "Moine", "Chevalier dragon", "Ninja", "Samouraï", "Faucheur", "Rôdeur vipère",
            "Barde", "Machiniste", "Danseur",
            "Mage noir", "Invocateur", "Mage rouge", "Pictomancien", "Mage bleu"
        ],
        ["Tank", "Soigneur", "DPS de mêlée", "DPS physique à distance", "DPS magique à distance"]),

        ("Guild Wars 2", 80,
        [
            "Gardien", "Guerrier", "Ingénieur", "Rôdeur", "Voleur",
            "Élémentaliste", "Nécromant", "Envoûteur", "Revenant"
        ],
        ["Soutien", "Soigneur", "DPS"]),

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
        List<string> existing = [.. context.Games.Select(game => game.Name)];

        if (Build(context, existing) > 0)
        {
            context.SaveChanges();
        }
    }

    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        List<string> existing = await context.Games.Select(game => game.Name).ToListAsync(cancellationToken);

        if (Build(context, existing) > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>Ajoute les jeux absents et renvoie leur nombre : le seed est additif, jamais destructif.</summary>
    private static int Build(ApplicationDbContext context, List<string> existingNames)
    {
        int added = 0;

        foreach ((string name, int maxLevel, string[] classes, string[] roles) in Catalogue)
        {
            if (existingNames.Contains(name))
            {
                continue;
            }

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
            added++;
        }

        return added;
    }
}
