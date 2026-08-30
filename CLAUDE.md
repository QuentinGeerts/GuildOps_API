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
- Un handler par cas d'usage, implémentant `IQueryHandler<TQuery, TResult>` ou `ICommandHandler<TCommand[, TResult]>` ; enregistrement explicite dans le DI, pas de scan
- Configuration EF par `IEntityTypeConfiguration<T>` — jamais d'attributs de mapping sur les entités
- Les conventions globales vivent dans `ConfigureConventions` du DbContext ; la seule qui change quelque chose aujourd'hui est `HaveMaxLength(256)` sur les `string` — sans elle tout part en `nvarchar(max)`, non indexable
- Tout script SQL brut touchant `GuildRanks` doit poser `SET QUOTED_IDENTIFIER ON` (ou `sqlcmd -I`) : SQL Server l'exige pour toute écriture sur une table portant un index filtré. EF Core le fait déjà, c'est `sqlcmd` qui ne le fait pas par défaut.
- Les dépassements du plafond de 256 sont explicités dans les configurations : `Guild.Description` 2000, `Guild.ChatUrl` 512, `GuildMembership.Note` 1000
- **Controllers MVC**, pas de Minimal API
- Le DbContext s'appelle `ApplicationDbContext`
- Un `DbSet` par racine d'agrégat uniquement (`Games`, `Players`, `Guilds`) — pas pour les entités enfants
- Un repository par racine d'agrégat, jamais par table ; aucune méthode ne renvoie `IQueryable`
- Les entités sans `DbSet` s'atteignent par `context.Set<T>()`, depuis le repository de leur racine d'agrégat
- **Ne jamais insérer par mutation d'une collection de navigation** : comme l'`Id` est généré dans le constructeur, EF voit une clé renseignée, marque l'entité `Modified` au lieu d'`Added`, émet un `UPDATE` et lève une `DbUpdateConcurrencyException`. Toujours passer par un `Add` explicite du repository
- Le remplacement d'une collection enfant se fait par différence explicite : charger l'agrégat avec suivi, puis `Add`/`Remove` sur le repository pour chaque écart
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
| Supprimer le personnage chef supprime la guilde | pas de guilde orpheline, et rien à archiver |
| Aucun historique d'adhésion | quitter une guilde supprime la ligne `GuildMembership`, sans trace |
| Un joueur peut avoir des personnages sur plusieurs jeux | `GameId` est porté par `Character`, jamais par `Player` |
| Pas de MediatR | CQRS écrit à la main : `IQueryHandler` / `ICommandHandler` dans `Application/Abstractions`, appel direct depuis le Controller |
| Argon2id pour le hachage | *memory-hard*, recommandé par l'OWASP ; package `Konscious.Security.Cryptography.Argon2` dans Infrastructure, hash au format PHC pour pouvoir relever les paramètres plus tard |
| Jetons JWT porteurs (HS256) | API sans état pour un front Angular ; `sub` = `PlayerId`, durée 60 min |
| Pas de jeton de rafraîchissement | simplification assumée : on se reconnecte à l'expiration |
| Clé de signature dans `appsettings.Development.json` | clé de développement uniquement ; la section `Jwt` est absente d'`appsettings.json`, donc l'application refuse de démarrer hors dev sans configuration explicite |
| La commande sert de DTO de requête | DataAnnotations (BCL, pas ASP.NET) portées par la commande ; `[ApiController]` valide avant d'atteindre le handler |
| … sauf si un champ ne doit pas venir du client | on sépare alors `XxxRequest` (lié au corps) et `XxxCommand` (construit par le Controller) — cas de `CreateCharacter`, dont le `PlayerId` vient du jeton |
| `MapInboundClaims = false` | les claims gardent leur nom JWT (`sub`) au lieu d'être traduits en URI WS-* |
| Grades socles à la création d'une guilde | `Chef de guilde` (tous droits, `IsLeader`), `Officier`, `Membre` (`IsDefault`) — définis dans `Application/Guilds/DefaultGuildRanks.cs` |
| Pas de statut sur les candidatures ni les invitations | accepter crée l'adhésion et supprime la ligne, refuser la supprime — cohérent avec « aucun historique d'adhésion » |
| Une invitation ne propose pas de grade | le nouveau membre reçoit le grade `IsDefault` de la guilde |
| Les droits de guilde se vérifient par `IGuildRepository.HasPermissionAsync` | une requête traverse adhésion → personnage → grade pour répondre « ce joueur a-t-il ce droit dans cette guilde » |
| Refuser une invitation accepte deux acteurs | l'invité décline, un gradé porteur de `InviteMember` annule — même route `DELETE`, même effet |
| Les collections d'un personnage se remplacent en bloc | `PUT` idempotent sur `/roles` et `/availabilities` : la liste envoyée devient l'état, ce qui colle à une grille de cases à cocher |
| Seed par `UseSeeding` / `UseAsyncSeeding` | permet d'utiliser les constructeurs du Domain, contrairement à `HasData` qui exige des valeurs figées dans la migration |
| Migration appliquée au démarrage, en Development seulement | `services.InitializeDatabaseAsync()` — c'est aussi ce qui déclenche le seed ; hors dev, la migration reste manuelle |
| Une violation d'index unique devient un résultat, pas une exception HTTP | `ApplicationDbContext` traduit l'erreur SQL 2601/2627 en `UniqueConstraintException` (aucune dépendance SQL dans Application), le handler en fait un `Outcome`, le Controller un code HTTP |
| `GameId` et `Server` d'une guilde viennent du personnage fondateur | la règle « même jeu, même serveur » devient structurelle : le client ne peut pas les fournir |
| « Introuvable » couvre aussi « pas à vous » | même réponse pour un personnage inexistant et pour celui d'un autre joueur, afin de ne pas révéler l'existence d'un identifiant |
| Résultat explicite plutôt qu'exception | un handler qui peut échouer renvoie un `...Result` avec un enum d'issue ; seul le Controller le traduit en code HTTP |

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
| `Guilds/GuildApplication.cs` | `GuildId`, `CharacterId`, `Message`, `CreatedAt` |
| `Guilds/GuildInvitation.cs` | `GuildId`, `CharacterId`, `Message`, `CreatedAt` |
| `Guilds/GuildPermission.cs` | enum : `ViewMembers`, `InviteMember`, `ReviewApplications`, `KickMember`, `AssignRank`, `ManageRanks`, `EditGuildProfile`, `WriteMemberNote` |
| `Games/GameRole.cs` | `GameId`, `Name`, `SortOrder`, `CreatedAt` |
| `Players/CharacterGameRole.cs` | `CharacterId`, `GameRoleId`, `CreatedAt` |
| `Players/Availability.cs` | `CharacterId`, `Day` (`System.DayOfWeek`), `Slot`, `CreatedAt` |
| `Players/TimeSlot.cs` | enum : `Morning`, `Afternoon`, `Evening` |

`Description` sur `Guild` est la présentation de la guilde ; `ChatUrl` est le lien Discord.
`Note` sur `GuildMembership` est une note libre sur le membre, rédigée par les gradés habilités.

`Permissions` sur `GuildRank` est une `List<GuildPermission>` : EF Core 10 la mappe seul, sans configuration, en collection primitive — un tableau JSON d'entiers dans une colonne `nvarchar(max)`.

### Phase 2 — faite

`GameRole` (par jeu), `CharacterGameRole` (n-n avec `Character`) et `Availability` (jour de la semaine + créneau,
portée par le **personnage**) sont écrites, configurées, migrées et pilotables par l'API.

---

## Les règles, et qui les garantit

**En base, par index unique ou contrainte :**

- `UNIQUE(CharacterId)` sur `GuildMembership` — un personnage n'appartient qu'à une guilde
- `UNIQUE(GuildId) WHERE IsLeader` — un seul chef de guilde
- `UNIQUE(GuildId) WHERE IsDefault` — un seul grade par défaut
- `UNIQUE(GuildId, Name)` et `UNIQUE(GuildId, SortOrder)` sur `GuildRank`
- `UNIQUE(GameId, Name)` sur `CharacterClass`
- `UNIQUE(GameId, Name)` sur `GameRole`
- `UNIQUE(CharacterId, GameRoleId)` sur `CharacterGameRole` — un rôle assigné une seule fois
- `UNIQUE(CharacterId, Day, Slot)` sur `Availability` — un créneau déclaré une seule fois
- `UNIQUE(GuildId, CharacterId)` sur `GuildApplication` et sur `GuildInvitation` — une seule demande en cours par couple
- `UNIQUE(Server, Name)` sur `Character` et sur `Guild`
- `CHECK (Level >= 1)` sur `Character` — la borne haute (`Game.MaxLevel`) n'est pas exprimable en `CHECK` : une contrainte ne lit que sa propre ligne

**Dans les handlers de la couche Application** (aucune contrainte SQL ne peut les exprimer) :

- Un personnage ne rejoint qu'une guilde du **même jeu et du même serveur** que lui
- La classe d'un personnage doit appartenir au **jeu de ce personnage**
- Le grade assigné à un membre doit appartenir à **sa** guilde
- Une guilde a toujours **exactement un** membre au grade `IsLeader` — le chef doit transférer la direction avant de partir
- Le grade `IsLeader` ne s'attribue ni ne se retire par `AssignRank`, et le chef ne peut pas être exclu : l'index filtré garantit *au plus un* chef, jamais *au moins un*
- Transférer la direction sera un cas d'usage à part, qui déplacera le grade des deux côtés en une transaction
- Une guilde naît avec ses grades socles et son chef, **dans une seule transaction**
- La borne haute de `Character.Level` : `Level <= Game.MaxLevel`
- Supprimer le personnage qui dirige une guilde supprime la guilde, ses grades et ses adhésions
- Supprimer un compte supprime ses personnages, donc les guildes qu'ils dirigent

Principe général : *invariant interne à une entité → l'entité ou le handler ; invariant qui dépend des autres lignes → la base, avec le handler pour produire un message clair.*

---

## État actuel

- Les 8 fichiers du Domain (phase 1) sont écrits.
- `GuildOps.Application` a `Abstractions/` et les tranches `Games/`, `Players/`, `Guilds/` : 5 requêtes et 4 commandes.
- `GuildOps.Infrastructure` a ses 8 configurations, `Persistence/Repositories/` (Game, Player, Guild), `Persistence/DatabaseSeeder.cs`, `Authentication/` (Argon2id, JWT, credentials).
- La base est seedée au démarrage en Development : Guild Wars 2 et World of Warcraft, 22 classes et 6 rôles au total.
- Le flux candidature est complet : candidater, lister, accepter, refuser — validé par un scénario de 15 vérifications.
- Le flux invitation est complet : inviter, lister des deux côtés, accepter, décliner ou annuler — validé par un scénario de 19 vérifications.
- Les rôles et les disponibilités d'un personnage sont pilotables : `PUT /api/characters/{id}/roles` et `/availabilities`, exposés sur la fiche — validé par un scénario de 12 vérifications.
- La gestion interne d'une guilde est en place : éditer le profil, attribuer un grade, annoter et exclure un membre — les quatre droits `EditGuildProfile`, `AssignRank`, `WriteMemberNote` et `KickMember` sont actifs, validés par 14 vérifications.
- `PlayerCredential` vit dans `Infrastructure/Authentication/`, avec sa configuration : `UNIQUE(Email)`, `UNIQUE(PlayerId)`, cascade depuis `Player`.
- `GuildOps.API` a `Controllers/` (`Games`, `Players`, `Auth`, `Characters`, `Guilds`) et `Extensions/ClaimsPrincipalExtensions.cs` ; `Program.cs` compose les deux couches, valide les jetons JWT, expose Scalar sur `/docs`.
- Le schéma a été validé sur une base jetable : les 9 contraintes se déclenchent, les deux index filtrés fonctionnent, aucun conflit de chemin de cascade (pas d'erreur 1785).
- Migration `InitialCreate` générée et appliquée : 8 tables, 18 index. Modèle et snapshot synchronisés.
- La base `GuildOps` est vide de données : `GET /api/games` renvoie `[]` tant que le seed n'est pas fait.

## Prochaine étape

1. Transférer la direction d'une guilde (seul chemin légitime pour changer de chef)
2. Rechercher des guildes : par jeu, serveur et nom, avec leur effectif
3. Quitter volontairement une guilde

Fait : le schéma complet et sa migration, le seed, l'inscription (Argon2id), la connexion (JWT), la création de personnage et de guilde, les lectures, les flux candidature et invitation, les rôles et disponibilités des personnages, et la gestion interne des guildes.

---

## Comment travailler avec moi

- Ne crée pas de fichiers que je n'ai pas demandés : pas de README spontané, pas de tests non sollicités.
- Si une décision d'architecture est ambiguë, pose-moi la question plutôt que de trancher seul.
- Avance fichier par fichier quand je te le demande, pas la solution entière d'un coup.
