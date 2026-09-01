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
- `Api.Tests` : tests d'intégration bout-en-bout via `WebApplicationFactory`, avec une base PostgreSQL de test (Testcontainers recommandé).
- Convention de nommage des tests xUnit classiques (`Domain.Tests`, `Api.Tests`) : `MethodName_Scenario_ExpectedResult` (ex: `AssignTask_UserNotParticipant_ThrowsDomainException`).

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
