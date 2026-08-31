# Suivi du projet — EventCo

**Ce fichier est la source de vérité de l'avancement du projet.**

## Instructions pour Claude Code

- Avant de commencer toute tâche, consulte ce fichier pour identifier la prochaine tâche `[ ]` non cochée du lot en cours.
- Après avoir terminé une tâche (code fonctionnel + testé), coche-la (`[x]`) et ajoute une ligne dans la section "Journal" en bas avec la date, la tâche terminée, et les fichiers/composants principaux créés ou modifiés.
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
- [ ] Configuration EF Core + première migration + `DbContext`
- [ ] Authentification passwordless — backend : génération et envoi du magic link (endpoint `POST /api/auth/request-link`)
- [ ] Authentification passwordless — backend : validation du token et création de session (endpoint `POST /api/auth/verify`)
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

> Ajouter une ligne par tâche terminée, format : `- AAAA-MM-JJ — [Lot X] Description de la tâche — fichiers/composants principaux`

- 2026-08-31 — [Lot 1] Initialisation de la solution .NET (Domain / Application / Infrastructure / Api + projets de tests xUnit), câblage des références selon la règle de dépendance Clean Architecture, build et `dotnet test` vérifiés OK — `EventCo.slnx`, `src/EventCo.Domain`, `src/EventCo.Application`, `src/EventCo.Infrastructure`, `src/EventCo.Api`, `tests/EventCo.Domain.Tests`, `tests/EventCo.Application.Tests`, `tests/EventCo.Api.Tests`
- 2026-08-31 — [Lot 1] Initialisation du projet React (Vite + TypeScript + Tailwind CSS v4 via `@tailwindcss/vite`), nettoyage du boilerplate Vite, page d'accueil minimale, build (`tsc -b && vite build`) et serveur de dev vérifiés OK — `client/` (structure feature-based à mettre en place au fur et à mesure des features du Lot 2), `client/vite.config.ts`, `client/src/index.css`, `client/src/App.tsx`, `.gitignore`
- 2026-08-31 — [Lot 1] Docker Compose (API + PostgreSQL + pgAdmin optionnel via profil `tools`), Dockerfile multi-stage pour l'API (.NET 10 SDK/ASP.NET), connexion string de dev locale ; build et démarrage des 3 services vérifiés OK (`docker compose build api`, `up`, healthcheck Postgres, `GET /openapi/v1.json` → 200) — `docker-compose.yml`, `src/EventCo.Api/Dockerfile`, `.dockerignore`, `.env.example`, `src/EventCo.Api/appsettings.Development.json`
- 2026-08-31 — [Lot 1] Modèle de données Domain : entités riches `User`, `MagicLinkToken`, `Event` (agrégat racine), `EventParticipant`, `EventTask`, value object `Email` (validation format), enums `EventStatus`/`ParticipantRole`/`TaskCategory`, classe de base `Entity` (égalité par Id) et `DomainException` ; règles métier encapsulées dans l'agrégat `Event` (créateur = organisateur auto-rejoint, promotion/rétrogradation/retrait réservés au créateur, assignation de tâche réservée aux participants) ; 25 tests unitaires xUnit (build + `dotnet test` vérifiés OK, solution complète compile) — `src/EventCo.Domain/Common/`, `src/EventCo.Domain/ValueObjects/Email.cs`, `src/EventCo.Domain/Users/User.cs`, `src/EventCo.Domain/Auth/MagicLinkToken.cs`, `src/EventCo.Domain/Events/`, `tests/EventCo.Domain.Tests/`
