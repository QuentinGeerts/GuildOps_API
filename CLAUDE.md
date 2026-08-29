# GuildOps — contexte du projet

API REST **ASP.NET Core 10 / .NET 10** avec **EF Core 10**, consommée par un front **Angular**.
Gestion de guildes de MMORPG : comptes joueurs, personnages, guildes, grades et droits.
Projet personnel d'apprentissage de la Clean Architecture.

**Réponds en français, avec un ton pédagogue.** Explique les choix d'architecture quand ils ne sont pas évidents.

---

## Architecture — 4 projets, à la racine (pas de dossier `src/`)

```
GuildOps.sln
GuildOps.Domain/          → aucune référence : ni projet, ni package NuGet
GuildOps.Application/     → Domain
GuildOps.Infrastructure/  → Application (+ EF Core)
GuildOps.API/             → Application + Infrastructure (SDK Web, Controllers)
```

**Règle absolue :** rien d'ASP.NET ni d'EF Core dans `Application`, rien du tout dans `Domain`.
`GuildOps.Domain.csproj` ne doit contenir aucun `<PackageReference>` — c'est le test qui compte, et il est vérifiable :

```bash
dotnet list GuildOps.Domain/GuildOps.Domain.csproj reference   # doit être vide
```

Chaque couche expose un `DependencyInjection.cs` avec sa méthode d'extension ; `Program.cs` se contente de les composer.

---

## Conventions de code — à respecter strictement

- Classes simples : **constructeur privé sans paramètre** (pour EF) + **constructeur public** qui génère l'`Id`
- `Guid` en direct, généré par `Guid.CreateVersion7()` — **pas** d'identifiants typés, **pas** de value objects
- `private set` sur `Id` et `CreatedAt` ; `{ get; set; }` public sur le reste
- Toute entité porte un horodatage de création : `CreatedAt` — sauf `GuildMembership`, qui le nomme `JoinedAt`
- `DateTimeOffset` pour toutes les dates, jamais `DateTime`
- `null!` sur les chaînes obligatoires, `= []` sur les collections, `?` sur l'optionnel
- Navigation inverse **uniquement** quand un écran charge le parent avec ses enfants — sinon la clé étrangère suffit
- **Aucune validation dans les entités** : elle vit dans les handlers de la couche Application
- Configuration EF par `IEntityTypeConfiguration<T>` — jamais d'attributs de mapping sur les entités
- Les conventions globales vivent dans `ConfigureConventions` du DbContext ; la seule qui change quelque chose aujourd'hui est `HaveMaxLength(256)` sur les `string` — sans elle tout part en `nvarchar(max)`, non indexable
- **Controllers MVC**, pas de Minimal API
- Le DbContext s'appelle `ApplicationDbContext`
- Un `DbSet` par racine d'agrégat uniquement (`Games`, `Players`, `Guilds`) — pas pour les entités enfants
- Un repository par racine d'agrégat, jamais par table ; aucune méthode ne renvoie `IQueryable`
- `SaveChanges` n'est **jamais** appelé dans un repository : c'est le rôle de `IUnitOfWork`, implémenté par le DbContext

---

## Décisions déjà prises — ne pas les remettre en cause

| Décision | Raison |
|---|---|
| Pas de value objects ni d'identifiants typés | choix assumé de classes simples |
| Pas d'ASP.NET Core Identity | table d'authentification écrite à la main, pour apprendre |
| `PlayerCredential` vit dans **Infrastructure** | un hash et un e-mail de connexion sont techniques, pas métier |
| Pas de traduction d'exceptions en HTTP | `[ApiController]` + DataAnnotations sur les DTO suffisent pour l'instant |
| Pas de dossier `src/`, pas de projet « sécurité » | structure volontairement plate |
| Les grades appartiennent à la **guilde**, pas au jeu | chaque guilde a sa propre hiérarchie |
| Les classes de personnage appartiennent au **jeu** | un personnage a une classe avant d'avoir une guilde |
| Pas de `Color` sur `CharacterClass` ni sur `GuildRank` | retiré volontairement : la couleur est un choix d'affichage, il vivra côté Angular |

---

## Le modèle métier

Un **joueur** (`Player`) possède plusieurs **personnages** (`Character`).
Un personnage choisit un **jeu** (`Game`) à sa création, ce qui détermine les **classes** disponibles (`CharacterClass`).
Un personnage peut créer une **guilde** ou en rejoindre une — **une seule à la fois**.
Une guilde appartient à un jeu et à un serveur, définit ses propres **grades** (`GuildRank`) qui portent des **droits** (`GuildPermission`), et regroupe des **adhésions** (`GuildMembership`).

### Phase 1 — entités déjà écrites dans `GuildOps.Domain`

| Fichier | Propriétés |
|---|---|
| `Games/Game.cs` | `Name`, `MaxLevel`, `CreatedAt`, `Classes` |
| `Games/CharacterClass.cs` | `GameId`, `Name`, `SortOrder`, `CreatedAt` |
| `Players/Player.cs` | `AccountName`, `CreatedAt`, `Characters` |
| `Players/Character.cs` | `PlayerId`, `GameId`, `CharacterClassId`, `Name`, `Server`, `Level`, `CreatedAt`, `Membership` |
| `Guilds/Guild.cs` | `GameId`, `Name`, `Server`, `Description`, `ChatUrl`, `CreatedAt`, `Ranks`, `Memberships` |
| `Guilds/GuildRank.cs` | `GuildId`, `Name`, `SortOrder`, `Permissions` (JSON), `IsLeader`, `IsDefault`, `CreatedAt` |
| `Guilds/GuildMembership.cs` | `GuildId`, `CharacterId`, `GuildRankId`, `Note`, `JoinedAt` |
| `Guilds/GuildPermission.cs` | enum : `ViewMembers`, `InviteMember`, `ReviewApplications`, `KickMember`, `AssignRank`, `ManageRanks`, `EditGuildProfile`, `WriteMemberNote` |

`Description` sur `Guild` est la présentation de la guilde ; `ChatUrl` est le lien Discord.
`Note` sur `GuildMembership` est une note libre sur le membre, rédigée par les gradés habilités.

`Permissions` sur `GuildRank` est une `List<GuildPermission>` : EF Core 10 la mappe seul, sans configuration, en collection primitive — un tableau JSON d'entiers dans une colonne `nvarchar(max)`.

### Phase 2 — pas encore commencée

`GameRole` (tank/heal/dps, par jeu), `CharacterGameRole` (n-n avec `Character`),
`Availability` (jour + matin/après-midi/soirée, portée par le **personnage**),
`GuildApplication` (candidature) et `GuildInvitation` (invitation).

---

## Les règles, et qui les garantit

**En base, par index unique ou contrainte :**

- `UNIQUE(CharacterId)` sur `GuildMembership` — un personnage n'appartient qu'à une guilde
- `UNIQUE(GuildId) WHERE IsLeader` — un seul chef de guilde
- `UNIQUE(GuildId) WHERE IsDefault` — un seul grade par défaut
- `UNIQUE(GuildId, Name)` et `UNIQUE(GuildId, SortOrder)` sur `GuildRank`
- `UNIQUE(GameId, Name)` sur `CharacterClass`
- `UNIQUE(Server, Name)` sur `Character` et sur `Guild`
- `CHECK` sur `Character.Level`, borné par `Game.MaxLevel`

**Dans les handlers de la couche Application** (aucune contrainte SQL ne peut les exprimer) :

- Un personnage ne rejoint qu'une guilde du **même jeu et du même serveur** que lui
- La classe d'un personnage doit appartenir au **jeu de ce personnage**
- Le grade assigné à un membre doit appartenir à **sa** guilde
- Une guilde a toujours **exactement un** membre au grade `IsLeader` — le chef doit transférer la direction avant de partir
- Une guilde naît avec ses grades socles et son chef, **dans une seule transaction**

Principe général : *invariant interne à une entité → l'entité ou le handler ; invariant qui dépend des autres lignes → la base, avec le handler pour produire un message clair.*

---

## État actuel

- Les 8 fichiers du Domain (phase 1) sont écrits.
- `GuildOps.Application` ne contient qu'un `DependencyInjection.cs` — pas encore de dossier `Abstractions/`.
- `GuildOps.Infrastructure` a `Persistence/ApplicationDbContext.cs` et un dossier `Persistence/Configurations/` vide.
- `GuildOps.API` a un dossier `Controllers/` vide ; `Program.cs` compose les deux couches, expose Scalar sur `/docs`, sans authentification.
- Aucune migration générée.

## Prochaine étape

1. `Infrastructure/Persistence/Configurations/` — un `IEntityTypeConfiguration<T>` par entité, avec les index listés plus haut
2. Compléter `ApplicationDbContext` avec ses trois `DbSet`
3. Générer la première migration
4. `Application/Abstractions/` — `IUnitOfWork`, `IPlayerRepository`, `IGuildRepository`

---

## Comment travailler avec moi

- Ne crée pas de fichiers que je n'ai pas demandés : pas de README spontané, pas de tests non sollicités.
- Si une décision d'architecture est ambiguë, pose-moi la question plutôt que de trancher seul.
- Avance fichier par fichier quand je te le demande, pas la solution entière d'un coup.
