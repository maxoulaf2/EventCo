# Suivi du projet — EventCo

**Ce fichier est la source de vérité de l'avancement du projet.**

## Instructions pour Claude Code

- Avant de commencer toute tâche, consulte ce fichier pour identifier la prochaine tâche `[ ]` non cochée du lot en cours.
- Après avoir terminé une tâche (code fonctionnel + testé), coche-la (`[x]`) et ajoute une ligne à la fin du fichier `journal-avancement.md` avec la date, la tâche terminée, et une décision notable le cas échéant (pas d'énumération de fichiers).
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
- [x] Cookie de session en `SameSite=None` pour les appels cross-origin réels en dev (`http://localhost:5173` → `https://localhost:7166`)
  > Tâche identifiée en cours de route (rule 5, 2026-09-08) : point resté en suspens depuis la note du 2026-09-01 ci-dessus ("nécessitera CORS + `SameSite=None`"), jamais traité depuis — le cookie était resté en `SameSite=Lax`. Cf. journal du 2026-09-08 (tâche « Frontend : page de création d'événement ») pour le détail de la découverte et de la correction.
  > Revenu sur cette décision le 2026-09-16, cf. tâche ci-dessous : `SameSite=None` posait un nouveau problème (cookie tiers bloqué en navigation privée), corrigé en repassant en same-origin plutôt qu'en cross-origin.
- [x] Passage à un frontend/API same-origin en dev (proxy Vite `/api` + `/hubs` vers l'API) et cookie de session repassé en `SameSite=Lax` (`Secure` conditionné à l'environnement)
  > Tâche identifiée en cours de route (rule 5, 2026-09-16), suite à un bug remonté par le développeur : en navigation privée, le lien magique connectait bien l'utilisateur mais la page des événements ne chargeait jamais et renvoyait vers le login. Cause : le cookie `SameSite=None`/`Secure` posé pour le cross-origin dev (`:5173` en http → `:7166` en https, schémas différents donc "cross-site" au sens schemeful-same-site) est traité comme cookie tiers par les navigateurs, bloqué par défaut en navigation privée (ITP Safari, ETP stricte de Firefox en privé, Chrome Incognito) — `GET /api/auth/me` répondait donc 401 après la redirection du lien magique. Plutôt que de rester dépendant de l'autorisation des cookies tiers (fragile, et en voie de disparition même hors navigation privée), le frontend et l'API sont rendus same-origin du point de vue du navigateur via un proxy Vite (`client/vite.config.ts`, cible configurable via `VITE_API_PROXY_TARGET`) ; `API_BASE_URL` (`client/src/shared/lib/api.ts`) passe en relatif par défaut. Le cookie repasse en `SameSite=Lax`, avec `Secure` désactivé uniquement en `Development` (la connexion navigateur ↔ Vite y reste en http).
- [x] Infrastructure de tests frontend en Gherkin (comportement avec back mocké, non-régression visuelle multi-tailles, E2E nominal)
  > Tâche identifiée en cours de route (rule 5), à la demande explicite du développeur.
- [x] Middleware d'authentification (lecture du cookie de session, résolution de l'utilisateur courant)
- [x] Service d'envoi d'email configuré (Resend/SendGrid/Mailtrap en dev)
- [x] CI (GitHub Actions) : exécution de tous les tests à chaque push sur `main`
  > Tâche demandée explicitement par le développeur avant de démarrer le lot 2, pas de déploiement automatisé pour l'instant (pas encore d'hébergement).


## Lot 2 — Gestion des événements

- [x] Endpoint + Command : création d'un événement (`POST /api/events`)
- [x] Endpoint + Query : consultation d'un événement (`GET /api/events/{id}`)
- [x] Endpoint + Query : liste des événements de l'utilisateur courant (`GET /api/events`)
  > Tâche identifiée en cours de route (rule 5), à la demande explicite du développeur : ni `cadrage-projet-eventco.md` ni ce backlog ne prévoyaient d'endpoint de listage, alors que la "Vue événement centralisée" (Must have, cadrage §4) en dépend pour être atteignable — sans lui, aucun utilisateur ne peut retrouver les événements auxquels il participe une fois créés/rejoints. Inclut la distinction "invitation en attente" (`EventParticipant.JoinedAt` null) vs "participation confirmée", nécessaire pour le flow d'invitation à venir plus bas dans ce lot.
- [x] Frontend : page d'accueil listant les événements de l'utilisateur (tableau de bord après connexion)
  > Tâche identifiée en cours de route (rule 5), même contexte que ci-dessus : aucune route n'existait après la connexion (`VerifyMagicLinkPage` n'était qu'un écran de confirmation sans destination).
- [x] Endpoint + Command : modification d'un événement (`PUT /api/events/{id}`)
- [x] Endpoint + Command : suppression d'un événement (réservé au créateur)
- [x] Endpoint + Command : invitation d'un participant par email
- [x] Logique métier : distinction créateur / co-organisateur / participant (règles d'autorisation dans l'agrégat `Event`)
- [x] Endpoint + Command : promotion/rétrogradation d'un participant en co-organisateur (réservé au créateur)
- [x] Frontend : page de création d'événement
- [x] Frontend : page de détail d'un événement (infos + liste des participants)
- [x] Frontend : formulaire d'invitation de participants
- [x] Endpoint + Command : indication du statut de participation par le participant lui-même (`PUT /api/events/{id}/participation-status`) + dropdown frontend sur la page de détail (« Je viens ! » / « Je ne viens pas » / « Je ne sais pas encore si je viens », statut par défaut `Unknown` pour un participant invité)
  > Tâche identifiée en cours de route (rule 5, 2026-09-18), à la demande explicite du développeur : ni `cadrage-projet-eventco.md` ni ce backlog ne prévoyaient de statut de participation — distinct du flow d'invitation existant (`EventParticipant.JoinedAt`/`HasJoined`, qui ne reflète que l'acceptation de l'invitation, pas l'intention de présence).
  > Corrigé le même jour (rule 5) : le créateur de l'événement ne voit pas ce dropdown et ne peut pas modifier son propre statut, fixé à `Attending` par défaut (garde appliquée côté Domain, pas seulement côté UI). Portée limitée à `CreatedByUserId` (pas à tout `Role == Organizer`) — à confirmer si les co-organisateurs promus doivent aussi être concernés.

## Lot 3 — Tâches et temps réel

- [x] Endpoint + Command : création d'une tâche (`POST /api/events/{id}/tasks`)
- [x] Endpoint + Command : assignation d'une tâche à un participant
- [x] Endpoint + Command : marquer une tâche comme faite/non faite
- [x] Endpoint + Command : suppression d'une tâche
- [x] Configuration SignalR : Hub `EventHub`, groupement des connexions par `EventId`
- [x] Diffusion temps réel des événements de tâches (création, assignation, statut, suppression) vers le groupe SignalR concerné
- [x] Refactoring : pattern Domain Events (mediator maison) pour les handlers de tâches + Unit of Work (`SaveChangesAsync` unique par requête)
  > Tâche identifiée en cours de route (rule 5, 2026-09-08), à la demande explicite du développeur : les handlers `CreateTask`/`AssignTask`/`CompleteTask`/`ReopenTask`/`DeleteTask` orchestraient directement la persistance (`IEventRepository`) et la notification SignalR (`ITaskRealtimeNotifier`) ; `EventRepository`/`UserRepository`/`MagicLinkTokenRepository` appelaient chacun `SaveChangesAsync` en interne à chaque écriture. Objectif : agrégat `Event` lève des domain events consommés après un `SaveChangesAsync` unique en fin de requête, pour ne plus dépendre du notifier SignalR dans les handlers (cf. `docs/conventions-code.md` §1.2).
- [x] Refactoring : repository pattern `ApplyAsync` piloté par les domain events (remplace `AddAsync`/`UpdateAsync`)
  > Tâche identifiée en cours de route (rule 5, 2026-09-10), à la demande explicite du développeur : `EventRepository.UpdateAsync` persistait via un diff générique de tout l'agrégat (`EventMapper.ApplyToEntity`/`SyncChildren`, toutes les propriétés + collections `Participants`/`Tasks` comparées à chaque écriture, quel que soit le changement réel). Objectif : une seule méthode `ApplyAsync` par repository, qui inspecte les domain events de l'agrégat pour déclencher des opérations de persistance précises et ciblées (cf. `docs/conventions-code.md` §1.2).
- [x] Endpoint + Query : liste des tâches d'un événement (`GET /api/events/{id}/tasks`)
  > Tâche identifiée en cours de route (rule 5, 2026-09-15) : ni `cadrage-projet-eventco.md` ni ce backlog ne prévoyaient d'endpoint de lecture pour les tâches — seuls des endpoints de mutation existent (création/assignation/statut/suppression), et `GetEventByIdQuery`/`EventDetailResponse` n'exposent pas non plus `Event.Tasks`. Sans lui, la tâche suivante ("Frontend : liste des tâches avec filtre par catégorie") n'a aucune donnée à afficher. `Event.Tasks` est déjà chargé par `EventRepository.GetByIdAsync` (`.Include(e => e.Tasks)`), donc pas de changement d'infrastructure nécessaire.
- [x] Frontend : liste des tâches avec filtre par catégorie
- [x] Frontend : connexion au Hub SignalR et mise à jour réactive de la liste de tâches
- [x] Frontend : formulaire d'ajout de tâche (titre, catégorie, quantité)
- [x] Frontend : interaction rapide pour cocher une tâche (optimisée mobile)

## Lot 4 — Finitions MVP

- [x] Attribut `data-testid` sur tout composant/élément testable du frontend, sélection exclusive via ce test-id dans les trois couches de tests (comportement/visuel/E2E)
  > Tâche identifiée en cours de route (rule 5, 2026-09-17), à la demande explicite du développeur, pour fiabiliser la sélection des composants dans les tests (jusqu'ici par rôle/label/texte affiché, avec des contournements ponctuels pour lever des ambiguïtés). Cf. `docs/conventions-code.md` §2.3 pour la convention de nommage et les décisions associées.
- [x] Script de démarrage local en une commande (Docker + migrations + backend + frontend)
  > Tâche identifiée en cours de route (rule 5, 2026-09-18), à la demande explicite du développeur, pour éviter d'enchaîner manuellement `docker compose up`, `dotnet ef database update`, `dotnet run` et `npm run dev` à chaque session de dev. Cf. `docs/journal-avancement.md` du 2026-09-18 pour le détail.
- [x] Frontend : déplacement de la liste des participants et du formulaire d'invitation dans une modale (page de détail d'un événement)
  > Tâche identifiée en cours de route (rule 5, 2026-09-18), à la demande explicite du développeur, dans le cadre des maquettes Lemon (`feature/maquettes-lemon`) : la liste des participants/organisateurs et le formulaire d'invitation, jusqu'ici affichés en permanence sous les infos de l'événement, sont désormais dans une modale ouverte au clic sur le bandeau "participants". Cf. `docs/journal-avancement.md` du 2026-09-18 pour le détail.
- [x] Frontend : redirection automatique vers le tableau de bord si une session valide existe déjà (page de connexion)
  > Tâche identifiée en cours de route (rule 5, 2026-09-18), suite à un bug remonté par le développeur : `LoginPage` (route `/`) ne consultait jamais `GET /api/auth/me` et affichait donc systématiquement le formulaire de connexion, même avec un cookie de session valide. Cf. `docs/journal-avancement.md` du 2026-09-18 pour le détail.
- [x] Frontend : modale de gestion de compte au clic sur l'icône de profil (tableau de bord), avec pour l'instant un bouton de déconnexion (endpoint `POST /api/auth/logout`)
  > Tâche identifiée en cours de route (rule 5, 2026-09-18), à la demande explicite du développeur. Cf. `docs/journal-avancement.md` du 2026-09-18 pour le détail.
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
