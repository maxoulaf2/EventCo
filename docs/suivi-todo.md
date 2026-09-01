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
- [ ] Authentification passwordless — frontend : formulaire de saisie d'email + page de confirmation
- [ ] Middleware d'authentification (lecture du cookie de session, résolution de l'utilisateur courant)
- [ ] Service d'envoi d'email configuré (Resend/SendGrid/Mailtrap en dev)

## Lot 2 — Gestion des événements

- [ ] Endpoint + Command : création d'un événement (`POST /api/events`)
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