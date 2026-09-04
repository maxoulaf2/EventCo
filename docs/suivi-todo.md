# Suivi du projet — EventCo

**Ce fichier est la source de vérité de l'avancement du projet.**

## Instructions pour Claude Code

- Avant de commencer toute tâche, consulte ce fichier pour identifier la prochaine tâche `[ ]` non cochée du lot en cours.
- Après avoir terminé une tâche (code fonctionnel + testé), coche-la (`[x]`) et ajoute une ligne dans la section "Journal" en bas avec la date, la tâche terminée, et une décision notable le cas échéant (pas d'énumération de fichiers).
- Si une tâche est bloquée ou reportée, ne la coche pas, mais ajoute une note juste en dessous avec `> Bloqué : raison`.
- Ne passe au lot suivant que si toutes les tâches Must have du lot en cours sont cochées, sauf instruction contraire de l'utilisateur.
- Si tu identifies une tâche manquante ou nécessaire non listée, ajoute-la dans le lot concerné avant de la traiter, plutôt que de l'exécuter silencieusement.
- Se référer à `cadrage-projet-eventco.md` pour les spécifications fonctionnelles et `conventions-code.md` pour les règles d'architecture et de style.

---

## Lot 1 — Fondations

- [x] Initialisation de la solution .NET (Domain / Application / Infrastructure / Api) selon la structure définie dans `conventions-code.md`
- [x] Initialisation du projet React (Vite + TypeScript + Tailwind CSS)
- [x] Docker Compose : API + PostgreSQL (+ pgAdmin optionnel en dev)
- [x] Modèle de données : entités `User`, `MagicLinkToken`, `Event`, `EventParticipant`, `EventTask` (Domain)
- [x] Configuration EF Core + première migration + `DbContext`
- [x] Authentification passwordless — backend : génération et envoi du magic link (endpoint `POST /api/auth/request-link`)
- [x] Authentification passwordless — backend : validation du token et création de session (endpoint `POST /api/auth/verify`)
- [x] Authentification passwordless — frontend : formulaire de saisie d'email + page de confirmation
- [x] Configuration CORS de l'API pour les appels cross-origin du frontend en dev (`http://localhost:5173` → `https://localhost:7166`)
  > Tâche identifiée en cours de route (rule 5) : sans elle, les appels `fetch` du frontend vers l'API échouent silencieusement en cross-origin, y compris pour recevoir/renvoyer le cookie de session. Cf. note du 2026-09-01 dans `conventions-code.md`/journal ci-dessous.
- [x] Infrastructure de tests frontend en Gherkin (comportement avec back mocké, non-régression visuelle multi-tailles, E2E nominal)
  > Tâche identifiée en cours de route (rule 5), à la demande explicite du développeur.
- [x] Middleware d'authentification (lecture du cookie de session, résolution de l'utilisateur courant)
- [x] Service d'envoi d'email configuré (Resend/SendGrid/Mailtrap en dev)
- [x] CI (GitHub Actions) : exécution de tous les tests à chaque push sur `main`
  > Tâche demandée explicitement par le développeur avant de démarrer le lot 2, pas de déploiement automatisé pour l'instant (pas encore d'hébergement).


## Lot 2 — Gestion des événements

- [x] Endpoint + Command : création d'un événement (`POST /api/events`)
- [ ] Endpoint + Query : consultation d'un événement (`GET /api/events/{id}`)
- [ ] Endpoint + Command : modification d'un événement (`PUT /api/events/{id}`)
- [ ] Endpoint + Command : suppression d'un événement (réservé au créateur)
- [ ] Endpoint + Command : invitation d'un participant par email
- [ ] Logique métier : distinction créateur / co-organisateur / participant (règles d'autorisation dans l'agrégat `Event`)
- [ ] Endpoint + Command : promotion/rétrogradation d'un participant en co-organisateur (réservé au créateur)
- [ ] Frontend : page de création d'événement
- [ ] Frontend : page de détail d'un événement (infos + liste des participants)
- [ ] Frontend : formulaire d'invitation de participants

## Lot 3 — Tâches et temps réel

- [ ] Endpoint + Command : création d'une tâche (`POST /api/events/{id}/tasks`)
- [ ] Endpoint + Command : assignation d'une tâche à un participant
- [ ] Endpoint + Command : marquer une tâche comme faite/non faite
- [ ] Endpoint + Command : suppression d'une tâche
- [ ] Configuration SignalR : Hub `EventHub`, groupement des connexions par `EventId`
- [ ] Diffusion temps réel des événements de tâches (création, assignation, statut) vers le groupe SignalR concerné
- [ ] Frontend : liste des tâches avec filtre par catégorie
- [ ] Frontend : connexion au Hub SignalR et mise à jour réactive de la liste de tâches
- [ ] Frontend : formulaire d'ajout de tâche (titre, catégorie, quantité)
- [ ] Frontend : interaction rapide pour cocher une tâche (optimisée mobile)

## Lot 4 — Finitions MVP

- [ ] Audit et ajustement du responsive sur toutes les pages (mobile-first)
- [ ] Configuration PWA : `manifest.json`, icônes, service worker basique
- [ ] Test d'installation PWA sur mobile (Android/iOS)
- [ ] Notification email : invitation à un événement
- [ ] Notification email : tâche assignée
- [ ] Notification email : rappel avant l'événement (nécessite un job planifié, ex: Hangfire ou tâche planifiée simple)
- [ ] Revue globale de sécurité (validation des rôles sur chaque endpoint, expiration des tokens, cookies httpOnly)
- [ ] Revue globale des tests (couverture Domain/Application/Api)

## Lot 5 — Enrichissements (post-MVP, non priorisé)

- [ ] Sondage de date/lieu (vote entre plusieurs options)
- [ ] Budget partagé (suivi des dépenses, calcul des remboursements)
- [ ] Templates d'événements pré-remplis
- [ ] Photos partagées post-événement
- [ ] Catégories de tâches personnalisables par événement
- [ ] Commentaires / chat par événement

---

## Journal d'avancement

> Ajouter une ligne par tâche terminée, format : `- AAAA-MM-JJ — [Lot X] Description concise de la tâche (+ décision notable si applicable)`

- 2026-08-31 — [Lot 1] Initialisation de la solution .NET (Clean Architecture Domain/Application/Infrastructure/Api + projets de tests xUnit)
- 2026-08-31 — [Lot 1] Initialisation du projet React (Vite + TypeScript + Tailwind CSS v4)
- 2026-08-31 — [Lot 1] Docker Compose (API + PostgreSQL + pgAdmin optionnel)
- 2026-08-31 — [Lot 1] Modèle de données Domain : `User`, `MagicLinkToken`, `Event` (agrégat racine), `EventParticipant`, `EventTask`, règles métier encapsulées dans l'agrégat `Event`
- 2026-08-31 — [Lot 1] Remplacement des levées génériques de `DomainException` par 18 exceptions métier spécifiques, une par règle violée
- 2026-08-31 — [Lot 1] Abstraction de la résolution du temps : suppression des appels directs à `DateTime.UtcNow` dans le Domain au profit d'un `IDateTimeProvider` injecté
- 2026-08-31 — [Lot 1] Configuration EF Core (`DbContext`, mappings, première migration `InitialCreate`) appliquée sur PostgreSQL
- 2026-08-31 — [Lot 1] Authentification passwordless (backend) — génération et envoi du magic link, `POST /api/auth/request-link`. **Décision** : MediatR étant passé sous licence payante en production, remplacé par une abstraction maison (`ICommand`/`ICommandHandler`/`ICommandDispatcher`) à utiliser pour tous les futurs Commands/Queries jusqu'à décision définitive du développeur
- 2026-08-31 — [Lot 1] Remplacement des tests xUnit+Moq du Command `RequestMagicLink` par des tests Gherkin/Reqnroll (InMemory, sans mock de repository) — convention actée dans `conventions-code.md` §1.4
- 2026-09-01 — [Lot 1] Premier test d'intégration bout-en-bout `EventCo.Api.Tests` pour `POST /api/auth/request-link`, contre un vrai PostgreSQL (Testcontainers)
- 2026-09-01 — [Lot 1] Extension de l'approche Gherkin/Reqnroll à `EventCo.Api.Tests` — conteneur PostgreSQL et `WebApplicationFactory` désormais démarrés une seule fois pour tout le run
- 2026-09-01 — [Lot 1] Authentification passwordless (backend) — validation du token et création de session, `POST /api/auth/verify` : token consommé (`MagicLinkToken.Consume`), compte `User` créé automatiquement à la première connexion (réutilisé sinon), cookie de session httpOnly déposé. **Décisions notables** :
  - Abstraction `ICommand`/`ICommandHandler`/`ICommandDispatcher` étendue avec un pendant à réponse (`ICommand<TResponse>`/`ICommandHandler<TCommand,TResponse>`/`Send<TResponse>`), nécessaire dès ce premier Command qui doit renvoyer un résultat à l'API (utile aussi pour les futures Queries du lot 2). Les deux interfaces `ICommand`/`ICommand<TResponse>` sont volontairement indépendantes (pas d'héritage) pour éviter une ambiguïté de surcharge sur `Send` côté `CommandDispatcher`.
  - Session = jeton signé maison (payload + signature HMAC-SHA256, même famille que le hash des magic links) plutôt qu'une dépendance JWT (`System.IdentityModel.Tokens.Jwt`) — le cadrage laisse le choix ("JWT ou identifiant de session") et ça évite d'introduire une nouvelle lib externe pour ce besoin (cf. prudence licences/maintenance actée précédemment). Secret + durée de vie configurables via la section `Session` (`appsettings.json`, à surcharger en production).
  - Cookie posé en `SameSite=Lax`/`Secure`/`HttpOnly` ; **point à retrancher lors de la tâche frontend** : le frontend Vite (`:5173`) appelant l'API (`:5000`) en cross-origin en dev nécessitera CORS + `SameSite=None` (donc HTTPS même en dev) pour que le cookie soit accepté — non traité ici, hors périmètre backand seul.
  - `EventCo.Api.Tests` : le port `IEmailSender` (jusqu'ici non substitué, seul `LoggingEmailSender` réel était utilisé) est désormais remplacé par un double observable (`RecordingEmailSender`) via `ConfigureTestServices` dans `ApiWebApplicationFactory`, seul moyen de récupérer le token brut du lien de connexion pour enchaîner sur la vérification dans un test bout-en-bout (le hash seul est stocké en base, par conception).
- 2026-09-01 — [Lot 1] Authentification passwordless (frontend) — formulaire de saisie d'email (`RequestMagicLinkForm`) + page de confirmation (`CheckEmailPage`), ainsi que la page de traitement du lien reçu par email (`VerifyMagicLinkPage`, route `/auth/verify?token=...`, indispensable pour que le flow magic link soit utilisable de bout en bout — non listée séparément dans le backlog mais découle directement de `MagicLink:VerificationUrlBase`). **Décisions notables** :
  - Ajout de `react-router-dom` (routage `/`, `/auth/check-email`, `/auth/verify`) et `@tanstack/react-query` (mutations `useRequestMagicLink`/`useVerifyMagicLink`), conformément à `conventions-code.md` §2.2 — rien n'était encore installé.
  - Organisation `features/auth/{components,hooks,api.ts,types.ts}` + `shared/lib/api.ts` (wrapper `fetch` : base URL via `VITE_API_URL`, `credentials: 'include'`, extraction du message d'erreur depuis `ProblemDetails`/`ValidationProblemDetails`).
  - Ajout de `"strict": true` dans `tsconfig.app.json`/`tsconfig.node.json` : absent du scaffold initial alors que `conventions-code.md` §2.2 l'exige — corrigé au passage plutôt que d'introduire du nouveau code non strict.
  - **Tâche manquante ajoutée et traitée** (rule 5) : configuration CORS côté API (`Program.cs` + section `Frontend:BaseUrl` dans `appsettings.json`), sans laquelle les appels `fetch` du frontend (`:5173`) vers l'API (`:7166` en https dev) sont bloqués par le navigateur — point explicitement laissé en suspens dans le journal du 2026-09-01 (tâche `VerifyMagicLink` backend).
  - Vérification de bout en bout faite manuellement en dev (API + Vite + Postgres réels, sans navigateur — outil non disponible dans cet environnement) : preflight CORS, `POST /api/auth/request-link`, lien loggé par `LoggingEmailSender`, `POST /api/auth/verify` (cookie `eventco_session` déposé, `Set-Cookie` correct), et réutilisation du même token → 400 `MagicLinkTokenAlreadyConsumedException` bien renvoyé. Le message d'erreur brut du backend (contient `TokenId`/dates) n'est volontairement pas affiché tel quel à l'utilisateur : `VerifyMagicLinkPage` affiche un message générique en cas d'échec.
  - Les messages de validation FluentValidation (ex: email invalide) restent en anglais (pas de configuration de localisation côté backend) — hors périmètre de cette tâche, atténué par la validation HTML5 (`type="email"`, `required`) côté formulaire qui limite les cas où ce message apparaît.
- 2026-09-02 — [Lot 1] Infrastructure de tests frontend en Gherkin, trois couches distinctes (convention actée dans `conventions-code.md` §2.3) : comportement mocké (**Vitest** + Testing Library + **MSW**, Gherkin via `@amiceli/vitest-cucumber`), non-régression visuelle (**Playwright** `toHaveScreenshot` + **playwright-bdd**, projets `mobile`/`tablet`/`desktop`), E2E nominal (**Playwright** + **playwright-bdd** contre la vraie stack docker compose). Scénarios d'exemple écrits sur le flow magic link existant, les trois suites tournent et passent (vérifié en local, y compris l'E2E contre l'API + PostgreSQL réels démarrés le temps du test). **Décisions notables** :
  - Maintenance de `vitest-cucumber` et `playwright-bdd` vérifiée avant adoption (dernières releases récentes, projets actifs) conformément à la prudence actée après l'épisode MediatR.
  - Extraction de `AppRoutes` hors de `App` (`client/src/App.tsx`) pour permettre aux tests de rendre les routes avec un `MemoryRouter`/`QueryClient` de test sans dupliquer la déclaration des routes.
  - Piège rencontré : avec `vitest-cucumber`, chaque étape Gherkin (pas chaque scénario) est un test Vitest à part entière — un `afterEach` global aurait réinitialisé les handlers MSW entre deux étapes d'un même scénario. Reset déplacé dans `AfterEachScenario`, propre à chaque fichier `.steps.tsx`.
  - Piège rencontré : `loadFeature(path)` résout `path` relativement au fichier `.steps.tsx` appelant (pas au cwd), et le parser français retire les mots de liaison (`que`/`qu'`) du texte des étapes — les deux ont fait échouer les premiers essais avant d'être compris et documentés dans `conventions-code.md` §2.3 pour ne pas les re-découvrir.
  - Les captures de référence de la couche visuelle sont sorties de `.features-gen/` (généré, ignoré par git) vers `tests/visual/__screenshots__/` (versionné) via `snapshotPathTemplate`, sans quoi elles auraient été perdues à chaque régénération des specs.
- 2026-09-02 — [Lot 1] E2E nominal isolé de la base de dev, à la demande du développeur : `docker-compose.e2e.yml` dédié (services `postgres-e2e`/`api-e2e`, projet Compose `eventco-e2e`, ports `5433`/`5001`, volume jetable), démarré/arrêté automatiquement par `globalSetup`/`globalTeardown` de `tests/e2e/playwright.config.ts` — `npm run test:e2e` n'a plus besoin de `docker compose up -d postgres api` en préalable. **Décisions notables** :
  - Migration de la base `eventco_e2e` (neuve à chaque run, `down -v` en teardown) faite depuis `global-setup.ts` (`dotnet ef database update`, même commande que celle documentée dans CLAUDE.md pour la base de dev) — une première version ajoutait un flag `Startup:MigrateOnStartup` dans `Program.cs` pour migrer au démarrage de l'API, revenu en arrière à la demande du développeur : le code de prod ne doit pas porter de branche conditionnelle pour un besoin propre aux tests. `Program.cs` est donc resté inchangé.
  - `name: eventco-e2e` explicite en tête de `docker-compose.e2e.yml` : sans lui, Compose dérive le nom de projet du nom du dossier et les deux fichiers compose (situés côte à côte) partageraient par défaut le même projet, au risque d'interactions entre stacks.
  - Vérifié en conditions réelles : stack de dev (`eventco-postgres`/`eventco-api`) déjà démarrée en parallèle pendant le run e2e, restée intacte (conteneurs, ports, données) après le run et son teardown.
- 2026-09-02 — [Lot 1] Middleware d'authentification : lecture du cookie de session (`eventco_session`), résolution de l'utilisateur courant. **Décisions notables** :
  - Implémenté comme un vrai scheme d'authentification ASP.NET Core (`SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>`, `src/EventCo.Api/Auth/`) plutôt qu'un middleware ad hoc peuplant `HttpContext.Items` : ça permet d'utiliser `[Authorize]` sur les futurs endpoints protégés (lot 2) et les 401 automatiques, sans code à dupliquer. `ISessionTokenService` étendu avec `ValidateSessionToken` (vérification HMAC en temps constant via `CryptographicOperations.FixedTimeEquals`, puis expiration) — le type privé `SessionTokenPayload` de `SessionTokenService` est devenu le record public `SessionTokenData` (`EventCo.Application.Common.Interfaces`), réutilisé pour la création et la validation du token.
  - Ajout de `ICurrentUserService` (`EventCo.Application.Common.Interfaces`, implémenté côté Api via `HttpContextAccessor`/claims) : c'est le point d'accès à l'utilisateur courant dont les Commands du lot 2 auront besoin pour les règles d'autorisation créateur/co-organisateur/participant.
  - **Tâche manquante ajoutée et traitée** (rule 5) : endpoint `GET /api/auth/me` (`[Authorize]`, Query `GetCurrentUserQuery` suivant le pattern `ICommand<TResponse>`/`ICommandDispatcher` existant) — nécessaire à la fois pour vérifier le middleware de bout en bout (aucun endpoint protégé n'existait encore) et pour le besoin réel du frontend de restaurer une session existante au chargement de l'app (le cookie étant httpOnly, le frontend ne peut pas savoir seul si une session valide existe). `IUserRepository` complété avec `GetByIdAsync`.
  - Tests Gherkin/Reqnroll ajoutés dans `EventCo.Api.Tests` (`Auth/CurrentUser/`, même pattern que `Auth/Verify/`) : cookie de session valide (200 + email attendu), absence de cookie (401), cookie falsifié (401).
- 2026-09-02 — [Lot 1] Service d'envoi d'email configuré : `SmtpEmailSender` (`src/EventCo.Infrastructure/Emailing/`), un seul sender SMTP générique plutôt qu'un SDK par fournisseur — Mailtrap (dev), Resend et SendGrid exposent tous un relais SMTP standard, donc le même code fonctionne avec les trois selon la config (`Email:Smtp:*`, nouvelle section `EmailOptions`/`SmtpOptions`). **Décisions notables** :
  - Lib retenue : **MailKit** (MIT, activement maintenu par jstedfast) — vérifié avant adoption conformément à la prudence licences/maintenance actée après l'épisode MediatR.
  - Sélection à l'exécution entre `SmtpEmailSender` et l'existant `LoggingEmailSender` selon que `Email:Smtp:Host` est renseigné ou non (`DependencyInjection.AddInfrastructure`) : par défaut (`Host` vide dans `appsettings.json`), les emails restent seulement loggés — aucun risque d'envoi accidentel tant qu'un compte n'est pas explicitement configuré.
  - Les identifiants ne sont jamais commités : `UserSecretsId` initialisé sur `EventCo.Api` (`dotnet user-secrets init`), section `Email:Smtp` à renseigner en local via `dotnet user-secrets set` (commandes ajoutées dans `CLAUDE.md`) pour un compte Mailtrap, en production via variables d'environnement (Resend/SendGrid), même approche que `Session:Secret`.
  - Pas de tests dédiés à `SmtpEmailSender` : comme `LoggingEmailSender`, c'est une implémentation fine d'un port externe (`IEmailSender`), déjà substituée par `RecordingEmailSender` dans `Application.Tests`/`Api.Tests` — cohérent avec l'approche existante (`conventions-code.md` §1.4).
- 2026-09-03 — [Lot 1] CI (GitHub Actions), `.github/workflows/ci.yml`, sur push `main` : 4 jobs parallèles — backend (`dotnet test`, `Domain`/`Application`/`Api.Tests`, ce dernier via Testcontainers), frontend comportement (`npm test`), frontend non-régression visuelle, frontend E2E nominal. Pas encore de déploiement automatisé (pas d'hébergement à ce stade). **Décisions notables** :
  - `Api.Tests` (Testcontainers) et `test:e2e` (`docker-compose.e2e.yml`) tournent sans configuration Docker particulière : le daemon Docker est préinstallé sur les runners `ubuntu-latest`.
  - Job E2E : `dotnet-ef` installé en outil global dans le workflow (pas de manifest d'outils local dans le repo), version alignée sur `Microsoft.EntityFrameworkCore.Design` (`10.0.4`) — même commande `dotnet ef database update` que celle utilisée par `global-setup.ts` en local.
  - **Non-régression visuelle — changement de process** (à la demande du développeur) : le job `frontend-visual-tests` tourne dans l'image Docker officielle Playwright (`mcr.microsoft.com/playwright:v1.62.1-jammy`, tag aligné sur `@playwright/test`), et non plus sur le poste de dev (Windows) — le rendu des fonts diffère selon l'OS et aurait rendu les captures de référence incomparables entre local et CI. `npm run test:visual:update` ne doit donc plus être lancé en local ; un second workflow dédié (`update-visual-baselines.yml`, déclenchement manuel) régénère les captures dans le même environnement et les dépose en artefact téléchargeable — le développeur les compare et les committe lui-même (pas de commit automatique, la validation reste un geste humain explicite). En cas d'échec en CI, le rapport Playwright (captures attendue/obtenue/diff) est aussi remonté en artefact pour permettre cette comparaison depuis la pipeline. Documenté dans `conventions-code.md` §2.3.
  - Déclenchement limité à `push` sur `main` (portée demandée) : pas encore de vérification sur les Pull Requests avant fusion — à reconsidérer plus tard si utile, non traité ici faute de demande explicite.
  - **Corrections post mise en place** (constatées au premier run) :
    - Actions GitHub (`checkout`/`setup-dotnet`/`setup-node`/`upload-artifact`) bumpées en `v7`/`v6`/`v7`/`v7` : les versions initiales (`v4`) tournent encore sur Node 20, deprecated par GitHub au profit de Node 24.
    - Job E2E : `dotnet restore` manquant avant `dotnet ef database update` (appelé par `global-setup.ts`) — celui-ci build `EventCo.Api`/`EventCo.Infrastructure` et a donc besoin d'un `project.assets.json` déjà généré.
    - `tests/visual/playwright.config.ts` et `tests/e2e/playwright.config.ts` : `outputDir`/`reporter` html passés en chemins explicites (`path.join(import.meta.dirname, ...)`) — la résolution par défaut de Playwright ne pointait pas de façon fiable vers `tests/visual|e2e/playwright-report`, ce qui faisait échouer l'upload d'artefact de rapport en CI (`No files were found`) quand ces suites échouaient.
  - **Publication des rapports Playwright en échec sur GitHub Pages** (à la demande du développeur, pour consultation directe au navigateur sans télécharger/dézipper l'artefact) : nouveau job `publish-reports` dans `ci.yml`, déclenché uniquement si `frontend-visual-tests` et/ou `frontend-e2e-tests` échoue, republie le site Pages en entier à chaque run concerné (`/visual/`, `/e2e/`, `index.html` de sommaire) — un job qui n'échoue pas cette fois ne fournit pas de rapport, donc l'ancien rapport de ce job disparaît du site ; accepté explicitement (pas d'historique à conserver, un seul développeur qui consulte au fil de l'eau). **Point important signalé et accepté par le développeur** : sur un repo privé hors GitHub Enterprise Cloud, un site Pages n'est pas restreignable — il est accessible publiquement par son URL (non indexée/partagée) même si le repo reste privé.
    > Bloqué : nécessite une action manuelle unique du développeur — Settings → Pages → Build and deployment → Source = "GitHub Actions" (impossible à faire depuis le workflow lui-même). Le job échouera tant que ce n'est pas fait.
  - **`update-visual-baselines.yml` ouvre désormais une Pull Request** (à la demande du développeur, pour merger facilement les nouvelles captures) au lieu de ne déposer qu'un artefact à télécharger : découpé en 2 jobs — `regenerate-screenshots` (image Docker Playwright, inchangé, dépose toujours l'artefact `visual-baselines` en complément) puis `open-pull-request` (runner nu, `git`/`gh` déjà présents) qui synchronise `client/tests/visual/__screenshots__/` avec l'artefact, et si le diff n'est pas vide, pousse sur une branche fixe `chore/update-visual-baselines` (force-push, réécrite à chaque déclenchement) et ouvre une PR — ou réutilise celle déjà ouverte sur cette branche s'il y en a une, cohérent avec le choix déjà fait pour les rapports Pages (pas d'historique à conserver). `gh` n'étant pas présent dans l'image Playwright (seul `git` l'est), la partie PR est déportée sur `ubuntu-latest` plutôt que d'installer `gh` manuellement dans le conteneur.
- 2026-09-03 — [Lot 2] Création d'un événement (`POST /api/events`) : Command `CreateEventCommand`/`CreateEventCommandHandler` (pattern `ICommand<TResponse>` existant), créateur = utilisateur courant résolu via `ICurrentUserService` (endpoint `[Authorize]`), délègue à `Event.Create` (déjà présent et testé côté Domain depuis le lot 1) qui inscrit automatiquement le créateur comme participant `Organizer` ayant rejoint. Nouveau `IEventRepository`/`EventRepository` (`AddAsync` seulement pour l'instant — étendu au fil des tâches suivantes du lot, comme `IUserRepository` au lot 1). **Décisions notables** :
  - Pas de validation FluentValidation dupliquant la règle Domain (titre vide) au-delà de `NotEmpty` — cohérent avec l'approche déjà en place (ex: `VerifyMagicLinkCommandValidator`/`Token`), le Domain reste la source de vérité et le test associé existe déjà dans `EventCo.Domain.Tests`.
  - `EventsController.Create` renvoie `201` via `Created(uri, body)` avec une URI construite à la main (`api/events/{id}`) plutôt que `CreatedAtAction` : l'endpoint `GET /api/events/{id}` (tâche suivante de ce lot) n'existe pas encore pour que `CreatedAtAction` puisse le référencer.
  - **Piège rencontré et corrigé avant qu'il ne casse la CI** : le step Gherkin `Etant donné une session ouverte via l'API pour "..."`, nécessaire pour tester ce nouvel endpoint `[Authorize]`, existait déjà dans `CurrentUserSteps.cs` (lot 1) — l'ajouter une seconde fois dans les steps de `EventCo.Api.Tests/Events/Create` aurait rendu ce step ambigu pour Reqnroll (deux méthodes `[Binding]` différentes matchant le même texte). Extrait dans un binding partagé (`Support/AuthenticatedSessionSteps.cs`) qui pose le cookie obtenu dans une classe de contexte injectée par Reqnroll (`Support/SessionContext.cs`, partagée entre bindings d'un même scénario) — `CurrentUserSteps.cs` a été adapté pour consommer ce contexte au lieu de dupliquer la logique.
  - Vérifié en conditions réelles : `dotnet test` sur les trois projets de tests backend (`Domain.Tests`, `Application.Tests`, `Api.Tests` — ce dernier nécessitant Docker, démarré manuellement pour l'occasion, absent par défaut de cet environnement) tous verts, y compris les scénarios existants du lot 1 (non-régression de l'extraction du step partagé).