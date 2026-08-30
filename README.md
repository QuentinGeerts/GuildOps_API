# GuildOps

API REST de gestion de guildes de MMORPG : comptes joueurs, personnages, guildes, grades et droits.

Projet personnel d'apprentissage de la **Clean Architecture** en ASP.NET Core 10 / EF Core 10, destiné à être consommé par un front Angular.

---

## Démarrer

**Prérequis** : .NET 10 SDK, SQL Server LocalDB (installé avec Visual Studio).

### 1. Faire confiance au certificat de développement

À faire **une seule fois par machine**, avant tout le reste. Une fenêtre Windows demandera confirmation.

```bash
dotnet dev-certs https --trust
```

Sans cette étape, le navigateur refusera `https://localhost:7181` et l'application semblera cassée alors qu'elle tourne.

### 2. Lancer

```bash
dotnet run --project GuildOps.API
```

Depuis Visual Studio, `F5` avec le profil **https** fait la même chose et ouvre directement la documentation.

Au premier démarrage, l'application restaure les paquets NuGet, crée la base `GuildOps` sur LocalDB, applique les migrations et alimente le catalogue : Dofus, Final Fantasy XIV, Guild Wars 2, The Elder Scrolls Online et World of Warcraft, avec leurs 70 classes et leurs 18 rôles. Comptez deux ou trois minutes — les démarrages suivants sont immédiats.

Le seed est **additif** : il ne crée que les jeux absents. Pour en ajouter un, il suffit d'étoffer `DatabaseSeeder.Catalogue` et de relancer l'application.

Si le HTTPS pose problème, un profil sans certificat écoute sur `http://localhost:5248` :

```bash
dotnet run --project GuildOps.API --launch-profile http
```

### 3. Explorer

La documentation interactive est sur **`/docs`** (Scalar). `GET /api/games` doit renvoyer les cinq jeux : si c'est le cas, tout fonctionne.

### Premiers pas

Six appels suffisent à traverser l'authentification, la validation métier et une transaction multi-tables.

| | Appel | Ce qu'on en retient |
|---|---|---|
| 1 | `POST /api/players` | créer un compte |
| 2 | `POST /api/auth/login` | récupérer `accessToken` |
| 3 | bouton **Authorize** de Scalar | y coller le jeton |
| 4 | `GET /api/games/{id}` | relever un `characterClassId` |
| 5 | `POST /api/characters` | créer un personnage |
| 6 | `POST /api/guilds` | fonder une guilde — observer les trois grades créés automatiquement |

Quelques essais instructifs une fois ce parcours fait : créer un personnage de niveau 80 sur The Elder Scrolls Online, plafonné à 50 ; utiliser une classe appartenant à un autre jeu ; candidater à une guilde d'un autre serveur. Chaque refus correspond à une règle du domaine.

> **Attention** : il faut **arrêter l'API avant de recompiler**. Sinon la compilation échoue sur `MSB3021 : le fichier est utilisé par un autre processus` — message obscur, cause banale.

### Configuration

Les sections `Jwt` et `Cors` n'existent que dans `appsettings.Development.json`. Hors développement, l'application **refuse de démarrer** sans configuration explicite, plutôt que de tourner avec une clé de signature publique.

| Clé | Rôle |
|---|---|
| `ConnectionStrings:Database` | chaîne SQL Server |
| `Jwt:Key` | clé de signature HS256, 32 octets minimum |
| `Jwt:ExpiryMinutes` | durée du jeton d'accès (60) |
| `Jwt:RefreshExpiryDays` | durée du jeton de rafraîchissement (14) |
| `Cors:Origins` | origines autorisées, par défaut `localhost:4200` |

---

## Architecture

Cinq projets à la racine, sans dossier `src/`.

```
GuildOps.Domain/          → aucune référence : ni projet, ni package NuGet
GuildOps.Application/     → Domain
GuildOps.Infrastructure/  → Application (+ EF Core)
GuildOps.API/             → Application + Infrastructure
GuildOps.UnitTests/       → Application
```

**La règle absolue** : rien d'ASP.NET ni d'EF Core dans `Application`, rien du tout dans `Domain`. C'est vérifiable :

```bash
dotnet list GuildOps.Domain/GuildOps.Domain.csproj reference
```

Le CQRS est écrit à la main : un handler par cas d'usage, implémentant `IQueryHandler<TQuery, TResult>` ou `ICommandHandler<TCommand[, TResult]>`, appelé directement depuis le Controller. Pas de MediatR.

Les décisions d'architecture et leurs raisons sont consignées dans [`CLAUDE.md`](CLAUDE.md).

---

## Authentification

Jetons porteurs JWT (HS256), `sub` = identifiant du joueur.

1. `POST /api/players` — créer un compte
2. `POST /api/auth/login` — obtenir un couple accès + rafraîchissement
3. Envoyer `Authorization: Bearer <accessToken>` sur les routes protégées
4. `POST /api/auth/refresh` quand l'accès expire — le jeton présenté est révoqué et un nouveau couple est émis

Les mots de passe sont hachés en **Argon2id** au format PHC. Les jetons de rafraîchissement sont stockés hachés en SHA-256 et tournent à chaque usage.

---

## Endpoints

31 routes. La colonne **Droit** indique ce qui est exigé au-delà de l'authentification.

### Jeux — `/api/games`

| | Route | Droit | |
|---|---|---|---|
| `GET` | `/api/games` | — | liste des jeux |
| `GET` | `/api/games/{id}` | — | un jeu, ses classes et ses rôles |

### Comptes — `/api/players`

| | Route | Droit | |
|---|---|---|---|
| `POST` | `/api/players` | — | créer un compte |
| `GET` | `/api/players/me` | connecté | son profil et ses personnages, avec leur guilde |
| `GET` | `/api/players/me/invitations` | connecté | les invitations reçues |
| `DELETE` | `/api/players/me` | connecté | supprimer son compte |

### Authentification — `/api/auth`

| | Route | Droit | |
|---|---|---|---|
| `POST` | `/api/auth/login` | — | obtenir les deux jetons |
| `POST` | `/api/auth/refresh` | — | renouveler les deux jetons |
| `POST` | `/api/auth/logout` | — | révoquer un jeton de rafraîchissement |

### Personnages — `/api/characters`

Toutes protégées, et limitées à ses propres personnages.

| | Route | Droit | |
|---|---|---|---|
| `POST` | `/api/characters` | connecté | créer un personnage |
| `GET` | `/api/characters/{id}` | connecté | fiche : guilde, grade, rôles et disponibilités |
| `PUT` | `/api/characters/{id}/roles` | propriétaire | remplacer ses rôles |
| `PUT` | `/api/characters/{id}/availabilities` | propriétaire | remplacer ses créneaux |
| `DELETE` | `/api/characters/{id}` | propriétaire | supprimer le personnage |

### Guildes — `/api/guilds`

| | Route | Droit | |
|---|---|---|---|
| `GET` | `/api/guilds` | connecté | rechercher : `?gameId=&server=&name=` |
| `POST` | `/api/guilds` | connecté | fonder une guilde |
| `GET` | `/api/guilds/{id}` | connecté | fiche : grades et membres |
| `PUT` | `/api/guilds/{id}` | `EditGuildProfile` | éditer le profil |

**Candidatures** — un personnage demande à entrer.

| | Route | Droit | |
|---|---|---|---|
| `POST` | `/api/guilds/{id}/applications` | connecté | candidater |
| `GET` | `/api/guilds/{id}/applications` | `ReviewApplications` | les candidatures en cours |
| `POST` | `/api/guilds/{id}/applications/{characterId}/accept` | `ReviewApplications` | accepter |
| `DELETE` | `/api/guilds/{id}/applications/{characterId}` | `ReviewApplications` | refuser |

**Invitations** — la guilde propose à un personnage.

| | Route | Droit | |
|---|---|---|---|
| `POST` | `/api/guilds/{id}/invitations` | `InviteMember` | inviter |
| `GET` | `/api/guilds/{id}/invitations` | `InviteMember` | les invitations en cours |
| `POST` | `/api/guilds/{id}/invitations/{characterId}/accept` | propriétaire du personnage | accepter |
| `DELETE` | `/api/guilds/{id}/invitations/{characterId}` | propriétaire **ou** `InviteMember` | décliner ou annuler |

**Membres**

| | Route | Droit | |
|---|---|---|---|
| `PUT` | `/api/guilds/{id}/members/{characterId}/rank` | `AssignRank` | changer le grade |
| `PUT` | `/api/guilds/{id}/members/{characterId}/note` | `WriteMemberNote` | annoter |
| `DELETE` | `/api/guilds/{id}/members/{characterId}` | propriétaire **ou** `KickMember` | partir ou exclure |
| `PUT` | `/api/guilds/{id}/leader/{characterId}` | être le chef | transférer la direction |

### Les droits

Chaque guilde définit ses propres grades, porteurs de droits : `ViewMembers`, `InviteMember`, `ReviewApplications`, `KickMember`, `AssignRank`, `ManageRanks`, `EditGuildProfile`, `WriteMemberNote`.

À sa création, une guilde reçoit trois grades socles : **Chef de guilde** (tous les droits), **Officier** et **Membre** (grade par défaut des nouveaux arrivants).

---

## Invariants notables

Deux règles se partagent le travail entre la base et le code.

**La base garantit qu'il y a *au plus* un chef** — un index unique filtré `UNIQUE(GuildId) WHERE IsLeader`.

**Les handlers garantissent qu'il y en a *au moins* un** : le grade de chef ne peut être ni attribué ni retiré, le chef ne peut être ni exclu ni partir. Le seul chemin est le transfert de direction, qui échange les grades des deux membres en une transaction.

Autres règles portées par les handlers, faute de pouvoir l'être en SQL :

- un personnage ne rejoint qu'une guilde du même jeu et du même serveur
- la classe d'un personnage appartient au jeu de ce personnage
- `Level <= Game.MaxLevel` — une contrainte `CHECK` ne lit que sa propre ligne, elle ne peut pas atteindre `Game`
- supprimer le personnage qui dirige une guilde supprime la guilde

---

## Tests

108 tests unitaires couvrant les handlers de la couche `Application`, en isolant les repositories avec NSubstitute. Aucune base n'est nécessaire.

```bash
dotnet test
```

Avec la couverture :

```powershell
if (Test-Path GuildOps.UnitTests\TestResults) { Remove-Item -Recurse -Force GuildOps.UnitTests\TestResults }
```

```powershell
dotnet test '--collect:XPlat Code Coverage'
```

```powershell
reportgenerator "-reports:GuildOps.UnitTests\TestResults\**\coverage.cobertura.xml" "-targetdir:GuildOps.UnitTests\TestResults\report" "-reporttypes:TextSummary;Html" "-assemblyfilters:-GuildOps.UnitTests"
```

`Infrastructure` et `API` n'ont pas encore de tests : le mapping EF, les repositories, le hachage et le routage restent à couvrir par des tests d'intégration.

---

## Base de données

SQL Server, 15 tables, migrations EF Core dans `GuildOps.Infrastructure/Persistence/Migrations`.

```bash
dotnet ef migrations add <Nom> --project GuildOps.Infrastructure --startup-project GuildOps.API --output-dir Persistence/Migrations
```

En développement, les migrations s'appliquent au démarrage — inutile de lancer `database update`.

> **Attention** : tout script SQL brut touchant `GuildRanks` doit poser `SET QUOTED_IDENTIFIER ON` (ou utiliser `sqlcmd -I`). SQL Server l'exige pour écrire dans une table portant un index filtré. EF Core le fait déjà ; `sqlcmd` non.
