# Conventions de code — SaaS de planification d'événements

Ce document complète le document de cadrage (`cadrage-projet-eventco.md`) et sert de référence de style/architecture à fournir à Claude Code. Objectif : garantir une cohérence sur toute la durée du projet, même après un reset de contexte.

---

## 1. Architecture backend — Clean Architecture + DDD

### 1.1 Structure des projets (solution .NET)

```
EventCo.sln
├── src/
│   ├── EventCo.Domain/           # Entités, Value Objects, interfaces de domaine, règles métier pures
│   ├── EventCo.Application/      # Use cases (Commands/Queries), interfaces des services externes, DTOs
│   ├── EventCo.Infrastructure/   # Implémentation EF Core, envoi d'email, SignalR, accès externes
│   └── EventCo.Api/              # Controllers, configuration, middlewares, DI, Program.cs
└── tests/
    ├── EventCo.Domain.Tests/
    ├── EventCo.Application.Tests/
    └── EventCo.Api.Tests/        # Tests d'intégration (WebApplicationFactory)
```

**Règle de dépendance** : `Domain` ne dépend de rien. `Application` dépend de `Domain`. `Infrastructure` et `Api` dépendent de `Application` (et implémentent ses interfaces). Aucune dépendance ne doit remonter dans le sens inverse.

### 1.2 Concepts DDD appliqués

- **Entités riches** : les entités du `Domain` (ex: `Event`, `EventTask`) encapsulent leurs règles métier via des méthodes (ex: `event.AssignTask(taskId, userId)`) plutôt que d'exposer des setters publics sur toutes les propriétés. On évite le modèle "anemic domain model".
- **Value Objects** quand c'est pertinent (ex: `Email` comme Value Object avec validation de format, plutôt qu'un simple `string`).
- **Agrégats** : `Event` est l'agrégat racine pour `EventParticipant` et `EventTask`. Toute modification de ces sous-entités passe par l'agrégat `Event`, qui garantit la cohérence (ex: on ne peut pas assigner une tâche à un utilisateur qui n'est pas participant).
- **Repositories** : interfaces définies dans `Domain` ou `Application` (ex: `IEventRepository`), implémentées dans `Infrastructure` avec EF Core. Un repository par agrégat racine (pas un repository générique par table).
- **Use Cases explicites (CQRS léger)** : dans `Application`, chaque action métier est une classe dédiée (ex: `CreateEventCommand` / `CreateEventCommandHandler`, `GetEventByIdQuery` / `Handler`). MediatR est recommandé pour orchestrer ça proprement.
  > **Note (2026-08-31)** : MediatR est désormais sous licence payante en production (Lucky Penny Software, depuis 2025). En attendant une décision définitive du développeur, l'orchestration est assurée par une abstraction maison (`EventCo.Application/Common/Messaging/` : `ICommand`, `ICommandHandler<T>`, `ICommandDispatcher`) qui reproduit le strict nécessaire de l'API MediatR sans dépendance externe. **Tous les Commands des lots suivants doivent suivre ce pattern** jusqu'à révision de cette note.
  >
  > **Extension (2026-09-01)** : ajout d'un pendant à réponse — `ICommand<TResponse>` / `ICommandHandler<TCommand,TResponse>` / `ICommandDispatcher.Send<TResponse>(ICommand<TResponse>, ...)` — pour les use cases qui doivent renvoyer un résultat (ex: `VerifyMagicLinkCommand` renvoie les infos de session). À utiliser aussi pour les futures Queries du lot 2 (`GetEventByIdQuery`, etc.), plutôt que d'introduire une interface `IQuery` séparée. `ICommand` et `ICommand<TResponse>` sont volontairement sans lien d'héritage entre elles pour éviter une ambiguïté de surcharge sur `Send`.
- **Domain Events** (optionnel, à introduire si utile) : ex: `TaskAssignedDomainEvent` déclenché par l'agrégat, consommé ensuite pour notifier via SignalR ou envoyer un email, sans coupler le domaine à l'infrastructure.

### 1.3 Conventions de nommage (backend)
- Classes/interfaces : `PascalCase`. Interfaces préfixées par `I` (`IEventRepository`).
- Commands/Queries : suffixe explicite (`CreateEventCommand`, `GetEventByIdQuery`).
- DTOs exposés par l'API : suffixe `Dto` ou `Response`/`Request` (`EventResponse`, `CreateEventRequest`).
- Fichiers : un type public par fichier, nom de fichier = nom du type.
- Enums : au singulier (`EventStatus`, pas `EventStatuses`).

### 1.4 Tests
- `Domain.Tests` : tests unitaires purs sur les règles métier des entités/agrégats (pas de mock nécessaire, pas de DB).
- `Application.Tests` : tests d'intégration des Commands/Queries écrits en **Gherkin** (fichiers `.feature`, en français, exécutés via **Reqnroll** — le fork open-source activement maintenu de SpecFlow, dont le développement communautaire s'est arrêté en 2024) plutôt que des tests unitaires avec mocks (Moq/NSubstitute).
  > **Décision (2026-08-31)** : à la demande du développeur, pas de mocking de repositories — chaque Command/Query est testé de bout en bout via le vrai `ICommandDispatcher`/`CommandDispatcher`, la vraie validation FluentValidation, et un vrai `EventCoDbContext` (EF Core provider **InMemory**, pour la rapidité — cf. `tests/EventCo.Application.Tests/Support/ApplicationTestHostBuilder.cs`). Seuls les ports vers des systèmes externes (ex: `IEmailSender`, `IDateTimeProvider`) sont remplacés par de simples doublons de test (`tests/EventCo.Application.Tests/TestDoubles/`), pas par une librairie de mock. Un sous-ensemble de scénarios nominaux sera complété plus tard avec un vrai PostgreSQL (Testcontainers), en plus (pas en remplacement) de la majorité des tests en InMemory.
  >
  > Structure par feature : `Auth/RequestMagicLink/RequestMagicLink.feature` + `RequestMagicLinkSteps.cs` (classe `[Binding]`) à côté du Command testé. Les méthodes de step suivent le texte du Gherkin (pas la convention `MethodName_Scenario_ExpectedResult` ci-dessous, propre aux tests xUnit classiques).
- `Api.Tests` : tests d'intégration bout-en-bout écrits en **Gherkin/Reqnroll** (mêmes conventions que `Application.Tests` ci-dessus), via `WebApplicationFactory<Program>` + un vrai PostgreSQL démarré à la volée avec **Testcontainers** (`postgres:16-alpine`, cohérent avec `docker-compose.yml`).
  > **Décision (2026-09-01)** : à la demande du développeur, tous les tests d'`Api.Tests` suivent aussi l'approche Gherkin (pas seulement `Application.Tests`) — un vrai appel HTTP (`HttpClient` du `WebApplicationFactory`) contre l'API complète, avec un vrai PostgreSQL (pas d'InMemory ici, contrairement à `Application.Tests` : le but de cette couche est justement de vérifier le comportement contre le vrai moteur relationnel). Le conteneur Postgres + le `WebApplicationFactory` sont démarrés **une seule fois pour tout le run** via des hooks Reqnroll `[BeforeTestRun]`/`[AfterTestRun]` (`tests/EventCo.Api.Tests/Support/Hooks.cs`, `ApiWebApplicationFactory.cs`) plutôt qu'un `IClassFixture` xUnit par scénario, pour éviter de payer le coût de démarrage du conteneur à chaque scénario. Chaque scénario doit donc utiliser des données distinctes (ex: emails uniques) pour rester indépendant des autres, faute d'un reset de base entre scénarios (pas encore mis en place).
  >
  > Piège rencontré : comparer une propriété interne du Value Object `Email` dans un prédicat LINQ traduit en SQL (`u.Email.Value == ...`) échoue avec le provider Npgsql (`InvalidOperationException`, traduction impossible) — `HasConversion` n'apprend à EF Core à convertir que la propriété mappée dans son ensemble (`User.Email`), pas ses membres internes, donc `.Value` n'est pas traduisible. Le provider InMemory masque le problème car il évalue le prédicat côté client. Solution : comparer le Value Object entier plutôt que sa propriété interne (`u.Email == email`) — EF Core pousse alors les deux côtés à travers le converter et génère bien un `WHERE` SQL, sans matérialiser toute la table.
  >
  > Structure par feature : `Auth/RequestLink/RequestLink.feature` + `RequestLinkSteps.cs`, même organisation que `Application.Tests`.
- Convention de nommage des tests xUnit classiques (`Domain.Tests` uniquement, désormais) : `MethodName_Scenario_ExpectedResult` (ex: `AssignTask_UserNotParticipant_ThrowsDomainException`).

---

## 2. Architecture frontend — React

### 2.1 Structure des dossiers

```
src/
├── features/              # Un dossier par fonctionnalité métier (feature-based, pas type-based)
│   ├── events/
│   │   ├── components/
│   │   ├── hooks/
│   │   ├── api.ts          # Appels API spécifiques à la feature
│   │   └── types.ts
│   ├── tasks/
│   └── auth/
├── shared/
│   ├── components/         # Composants UI génériques réutilisables (Button, Modal...)
│   ├── hooks/
│   └── lib/                # Config axios/fetch, helpers, config SignalR
├── App.tsx
└── main.tsx
```

On privilégie une organisation **par feature** plutôt que par type technique (pas de gros dossier `components/` fourre-tout) : ça facilite le travail de Claude Code sur un périmètre isolé sans naviguer dans tout le projet.

### 2.2 Conventions
- Composants : `PascalCase`, un composant par fichier (`TaskList.tsx`).
- Hooks custom : préfixe `use` (`useEventTasks.ts`).
- Typage strict : TypeScript en mode `strict`, pas de `any` sauf cas justifié en commentaire.
- State management : commencer avec les hooks React natifs (`useState`, `useContext`) + **React Query** pour la gestion des appels API (cache, invalidation, loading states) — évite d'introduire Redux/Zustand tant que le besoin ne s'en fait pas sentir.
- Styles : Tailwind CSS exclusivement, pas de CSS-in-JS ni de fichiers `.css` séparés sauf cas exceptionnel (ex: animations complexes).
- Mobile-first : toute classe Tailwind responsive s'écrit du plus petit écran vers le plus grand (`class="text-sm md:text-base"`, jamais l'inverse).

### 2.3 Tests frontend
> **Décision (2026-09-02)** : à la demande du développeur, trois couches de tests distinctes, toutes écrites en **Gherkin** (`.feature` en français, `# language: fr`, même style que le backend §1.4), avec un outil différent par couche selon ce qu'elle vérifie.

| Couche | Outil | Commande | Ce qu'elle vérifie |
|---|---|---|---|
| Comportement (back mocké) | **Vitest** + React Testing Library + **MSW**, Gherkin via `@amiceli/vitest-cucumber` | `npm test` / `npm run test:watch` | Le rendu et les interactions du client, indépendamment du back (appels réseau interceptés) |
| Non-régression visuelle | **Playwright** (`toHaveScreenshot`) + **playwright-bdd** | `npm run test:visual` (cf. note CI ci-dessous pour `:update`) | L'apparence des pages à plusieurs tailles d'écran (projets `mobile`/`tablet`/`desktop`) |
| E2E nominal | **Playwright** + **playwright-bdd**, contre une stack dédiée | `npm run test:e2e` (tout inclus, cf. ci-dessous) | Le parcours réel de bout en bout, sans mock |

Structure : `.feature` + `.steps.tsx` colocalisés avec le composant testé pour la couche comportement (ex: `src/features/auth/RequestMagicLink.feature`) ; `tests/e2e/` et `tests/visual/` (chacun avec `features/`, `steps/`, `playwright.config.ts` propres) pour les deux couches Playwright, qui testent des parcours transverses plutôt qu'un composant isolé.

**E2E nominal — stack dédiée, isolée de la base de dev.** `npm run test:e2e` ne suppose plus `docker compose up -d postgres api` démarré à la main : `globalSetup`/`globalTeardown` (`tests/e2e/global-setup.ts`/`global-teardown.ts`) démarrent et arrêtent automatiquement `docker-compose.e2e.yml` (services `postgres-e2e`/`api-e2e`, projet Compose `eventco-e2e`, ports `5433`/`5001`, volume jetable) autour de la suite.
> **Décision (2026-09-02)** : à la demande du développeur, cette stack ne doit jamais interférer avec celle utilisée en dev (`docker compose up -d postgres api`, ports `5432`/`5000`) — ni la base de données, ni les conteneurs. D'où des noms de service, ports, `container_name` et **nom de projet Compose** (`name: eventco-e2e` en tête de fichier) tous distincts : sans ce `name` explicite, Compose dérive le nom de projet du nom du dossier et les deux fichiers (situés côte à côte) partageraient le même projet par défaut, au risque d'interactions entre les deux stacks. La base `eventco_e2e` étant neuve à chaque run, `globalSetup` la migre lui-même (`dotnet ef database update --project src/EventCo.Infrastructure --startup-project src/EventCo.Api --connection ...`, même commande que celle documentée dans CLAUDE.md pour la base de dev) plutôt que de faire migrer l'API elle-même à son démarrage : pas de branche « migre-toi » conditionnelle dans `Program.cs` pour un besoin qui n'appartient qu'aux tests — `Program.cs` reste identique quel que soit l'environnement qui le démarre. Le volume Postgres n'est pas nommé : `down -v` (en fin de run, y compris si les tests échouent) le supprime, donc chaque run repart d'une base vierge — vérifié en conditions réelles (stack de dev déjà démarrée en parallèle, restée intacte après le run e2e).

Points d'attention (rencontrés en mettant en place ces trois couches, à ne pas re-découvrir) :
- **`@amiceli/vitest-cucumber` : chaque étape Gherkin est un test Vitest à part entière** (pas juste chaque scénario). Un `afterEach(() => server.resetHandlers())` global (dans `src/test/setup.ts`) s'exécuterait donc *entre deux étapes* d'un même scénario et effacerait un `server.use(...)` avant l'étape qui en a besoin. Le reset des handlers MSW se fait par scénario via `AfterEachScenario` dans le fichier `.steps.tsx` lui-même, pas dans le setup global.
- **`loadFeature(path)` résout `path` relativement au fichier appelant** (le `.steps.tsx`), pas au cwd — utiliser un chemin relatif du type `./MonFichier.feature`, pas `./src/...`.
- **Les mots de liaison français (`que`, `qu'`) sont retirés du texte de l'étape par le parser** (`Etant donné que je suis sur la page de connexion` → l'étape à enregistrer est `'je suis sur la page de connexion'`, pas `'que je suis sur la page de connexion'`), aussi bien avec `vitest-cucumber` qu'avec `playwright-bdd`.
- **`playwright-bdd`** : les fixtures Playwright (`page`, `testInfo`, ...) sont toutes destructurées d'un seul objet (`({ page, testInfo }) => ...`), jamais passées en second argument comme avec `test()` natif.
- **Captures de référence (`toHaveScreenshot`)** : les fichiers `.feature.spec.js` sont générés (et régénérés) par `bddgen` dans `.features-gen/` (ignoré par git) — sans le configurer, les captures de référence y naîtraient aussi et seraient perdues à chaque régénération. `snapshotPathTemplate` dans `tests/visual/playwright.config.ts` les place hors de `.features-gen`, dans `tests/visual/__screenshots__/` (versionné).
- **E2E nominal** : `VITE_API_URL` doit pointer vers `api-e2e` (`http://localhost:5001`), ni vers l'API de dev (`5000`) ni vers le profil https local (`.env`, `7166`) — fixé par défaut dans `webServer.env` de `tests/e2e/playwright.config.ts`, surchargeable via variable d'environnement.

**Captures de référence (non-régression visuelle) — CI uniquement.**
> **Décision (2026-09-03)** : à la demande du développeur, `npm run test:visual:update` ne doit plus être exécuté en local pour committer de nouvelles références — le rendu des fonts diffère selon l'OS (le poste de dev est sous Windows) et casserait la comparaison une fois exécutée en CI sous Linux. La régénération se fait exclusivement via le workflow GitHub Actions `update-visual-baselines` (déclenchement manuel, `workflow_dispatch`), qui tourne dans la même image Docker Playwright (`mcr.microsoft.com/playwright:vX.Y.Z-jammy`, tag aligné sur la version de `@playwright/test`) que la CI (`.github/workflows/ci.yml`, job `frontend-visual-tests`). Il produit un artefact téléchargeable (`tests/visual/__screenshots__/`) : le développeur le récupère, compare visuellement les changements, puis committe lui-même s'ils sont valides — pas de commit automatique, la validation reste un geste humain explicite. En cas d'échec du job `frontend-visual-tests` en CI, le rapport Playwright (captures attendue/obtenue/diff) est lui aussi remonté en artefact pour permettre cette comparaison depuis la pipeline.

---

## 3. Conventions transverses

### 3.1 Git
- Branches : `feature/nom-court`, `fix/nom-court`.
- Commits : format court impératif (`Ajoute l'assignation de tâches`, pas `Added task assignment`), un commit = un changement logique.
- Un lot du découpage (voir cadrage, section 6) = idéalement une Pull Request, même en solo, pour garder un historique de review clair.

### 3.2 API
- REST classique, routes au pluriel (`/api/events`, `/api/events/{id}/tasks`).
- Codes de statut HTTP standards (201 à la création, 404 si non trouvé, 403 si droits insuffisants, 400 si validation échouée).
- Validation des requêtes en entrée via FluentValidation (cohérent avec l'approche Application/Commands).

### 3.3 Documentation dans le code
- Pas de commentaires qui répètent ce que le code dit déjà.
- Un commentaire est justifié quand il explique un **pourquoi** non évident (ex: une règle métier particulière, un contournement technique).

---

## 4. Utilisation avec Claude Code

- Ce document + `cadrage-projet-eventco.md` doivent être fournis en contexte à chaque nouvelle session Claude Code.
- Le fichier `suivi-todo.md` (voir document séparé) est la **source de vérité de l'avancement** : Claude Code doit le consulter avant de commencer une tâche, et le mettre à jour après chaque tâche terminée.
- En cas de doute d'architecture non couvert par ce document, Claude Code doit proposer une solution cohérente avec les principes ci-dessus plutôt que d'improviser un pattern différent.
