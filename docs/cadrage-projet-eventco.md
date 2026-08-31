# Document de cadrage — SaaS de planification d'événements de groupe

## 1. Contexte et objectifs

### 1.1 Objectif du projet
Projet personnel de montée en compétences, mené comme s'il s'agissait d'un projet professionnel : architecture moderne, séparation back/front claire, authentification, base de données relationnelle, temps réel, et delivery pipeline (CI/CD).

L'objectif secondaire (et non des moindres) est de pratiquer le développement assisté par IA (Claude Code) en délégation large, avec review humaine.

### 1.2 Pitch du produit
Un SaaS permettant d'organiser des événements de groupe (repas, anniversaires, week-ends entre amis...) en collaboration : créer un événement, inviter des participants, répartir les tâches (courses, logistique), et suivre l'avancement en temps réel.

### 1.3 Cas d'usage de référence
**Le repas de Noël entre amis** : un organisateur crée l'événement avec date et lieu fixés, invite ses amis par email, certains deviennent co-organisateurs. La liste de courses/tâches est partagée et chacun s'assigne ou se voit assigner des éléments ("qui apporte la bûche", "qui s'occupe de la déco"). Les mises à jour sont visibles en temps réel par tous.

### 1.4 Contrainte d'usage majeure
**La majorité des utilisateurs accèdent au service depuis un navigateur mobile.** Le design doit être pensé mobile-first, avec une ergonomie tactile soignée (zones cliquables larges, actions rapides, formulaires courts).

---

## 2. Stack technique

| Couche | Choix | Justification |
|---|---|---|
| Backend | **ASP.NET Core (C#)** | Expertise existante du développeur (8-10 ans), permet de se concentrer sur l'architecture moderne plutôt que d'apprendre un nouveau langage |
| Frontend | **React + Tailwind CSS** | Écosystème le plus large, génération de code IA la plus fiable ; Tailwind pour un responsive rapide et lisible en mobile-first |
| Temps réel | **SignalR** | Solution native .NET, s'intègre nativement au backend ASP.NET Core, compatible avec un client React |
| Base de données | **PostgreSQL + EF Core** | Standard robuste, excellent support EF Core, gratuit |
| Authentification | **Passwordless (magic link)** | Réduction de friction pour des invités occasionnels ; pas de gestion de mots de passe (hashing, reset, politique de complexité) |
| PWA | **Manifest + Service Worker basique** | Installable sur écran d'accueil mobile, tolérance réseau instable |
| Déploiement | À définir (pistes : Railway, Render, Azure App Service) | Non bloquant pour le développement initial |
| Tests | xUnit (backend) | Cohérent avec l'écosystème .NET |
| CI/CD | GitHub Actions (à affiner) | Standard, gratuit pour projets perso |

### 2.1 Flow d'authentification (magic link)
1. L'utilisateur saisit son email sur le formulaire de connexion.
2. Le backend génère un token unique à durée de vie courte (~15 min), le stocke hashé en base (`MagicLinkToken`), et envoie un email contenant un lien avec ce token (via un service comme Resend ou SendGrid).
3. L'utilisateur clique sur le lien → le backend vérifie la validité du token (non expiré, non consommé).
4. Une session est créée : cookie httpOnly contenant un JWT ou un identifiant de session, avec un refresh token pour maintenir la connexion dans la durée.
5. Si l'email ne correspond à aucun `User` existant, un compte est créé automatiquement (première connexion = inscription).

---

## 3. Modèle de données

### 3.1 Entités principales

**User**
| Champ | Type | Notes |
|---|---|---|
| Id | Guid | |
| Email | string | unique |
| DisplayName | string | saisi à la première connexion |
| AvatarUrl | string? | optionnel (could have) |
| CreatedAt | DateTime | |

**MagicLinkToken**
| Champ | Type | Notes |
|---|---|---|
| Id | Guid | |
| Email | string | pas nécessairement lié à un `User` existant |
| TokenHash | string | le token brut n'est jamais stocké en clair |
| ExpiresAt | DateTime | ex: +15 min |
| ConsumedAt | DateTime? | null tant que non utilisé |

**Event**
| Champ | Type | Notes |
|---|---|---|
| Id | Guid | |
| Title | string | ex: "Repas de Noël" |
| Description | string? | |
| EventDate | DateTime | fixée à la création (pas de sondage au MVP) |
| Location | string? | texte libre pour le MVP |
| CreatedByUserId | Guid | FK → User, le créateur |
| Status | enum | `Draft`, `Planned`, `Completed`, `Cancelled` |
| CreatedAt | DateTime | |

**EventParticipant** (table de liaison User ↔ Event)
| Champ | Type | Notes |
|---|---|---|
| Id | Guid | |
| EventId | Guid | FK |
| UserId | Guid | FK |
| Role | enum | `Organizer`, `Participant` |
| InvitedAt | DateTime | |
| JoinedAt | DateTime? | null tant que l'invité n'a pas confirmé via magic link |

**EventTask**
| Champ | Type | Notes |
|---|---|---|
| Id | Guid | |
| EventId | Guid | FK |
| Title | string | ex: "Bûche au chocolat" |
| Category | enum | `Courses`, `Logistique`, `Autre` |
| Quantity | string? | texte libre (ex: "2", "1kg") |
| AssignedToUserId | Guid? | null = non assignée |
| IsDone | bool | |
| CreatedAt | DateTime | |

### 3.2 Règles de gestion des rôles
- Le **créateur** (`Event.CreatedByUserId`) a les droits ultimes : suppression de l'événement, gestion des rôles des autres participants, retrait de n'importe qui.
- Un **co-organisateur** (`EventParticipant.Role = Organizer`) peut modifier les informations de l'événement, gérer les tâches, inviter de nouveaux participants — mais ne peut pas supprimer l'événement ni retirer le créateur.
- Un **participant** (`EventParticipant.Role = Participant`) peut consulter l'événement, s'auto-assigner des tâches, et cocher ses propres tâches comme faites.
- Le créateur possède automatiquement une entrée `EventParticipant` avec `Role = Organizer` ; la distinction "droits ultimes" se fait via la comparaison avec `Event.CreatedByUserId`.

---

## 4. Fonctionnalités du MVP (MoSCoW)

### Must have
- Authentification passwordless (magic link)
- Création d'événement (titre, description, date, lieu)
- Invitation de participants par email
- Distinction créateur / co-organisateur / participant
- Liste de tâches partagée avec catégories, quantités et assignation
- Mise à jour en temps réel (SignalR) des tâches entre participants
- Vue événement centralisée (résumé, participants, tâches)
- Design responsive mobile-first
- PWA (installation sur écran d'accueil, tolérance réseau)

### Should have
- Notifications par email (invitation, tâche assignée, rappel avant l'événement)
- Commentaires / chat par événement

### Could have
- Sondage de date/lieu (vote entre plusieurs options)
- Budget partagé (suivi des dépenses, calcul de qui doit combien à qui)
- Templates d'événements pré-remplis (ex: liste de courses type "Noël")
- Photos partagées post-événement
- Catégories de tâches personnalisables par événement

### Won't have (hors scope MVP)
- Application mobile native
- Paiement intégré (remboursements via Stripe, etc.)
- Multi-langue

---

## 5. Contraintes transverses

- **Responsive / mobile-first** : conception et CSS pensés d'abord pour petit écran (Tailwind CSS), adaptation ensuite vers desktop.
- **Ergonomie tactile** : zones cliquables larges, actions rapides (ex: cocher une tâche en un geste), formulaires courts adaptés à la saisie mobile.
- **Temps réel** : toute mise à jour d'une tâche (création, assignation, statut) doit être propagée instantanément à tous les participants connectés via SignalR, groupés par événement (un "groupe SignalR" par `EventId`).
- **Sécurité** : tokens de magic link hashés en base, cookies de session en httpOnly, validation stricte des droits par rôle sur chaque action API.

---

## 6. Découpage proposé en lots de développement

Ce découpage vise à séquencer le travail de façon à obtenir rapidement une verticale fonctionnelle testable, avant d'enrichir.

**Lot 1 — Fondations**
- Setup du repo (structure back .NET / front React), Docker Compose (API + PostgreSQL)
- Modèle de données + migrations EF Core
- Authentification passwordless (backend + UI)

**Lot 2 — Gestion des événements**
- CRUD événement (création, consultation, modification, suppression)
- Gestion des participants et rôles (invitation par email, acceptation)

**Lot 3 — Tâches et temps réel**
- CRUD des tâches (catégorie, quantité, assignation, statut)
- Intégration SignalR pour la synchronisation en temps réel

**Lot 4 — Finitions MVP**
- Responsive complet + PWA (manifest, service worker)
- Notifications email (invitation, assignation, rappel)

**Lot 5 — Enrichissements (post-MVP)**
- Sondage date/lieu, budget partagé, templates d'événements, chat

---

## 7. Notes pour l'utilisation avec l'IA (Claude Code)

- Ce document sert de contexte de référence à fournir à Claude Code en début de session.
- Le mode de collaboration privilégié est la **délégation large** : Claude Code peut proposer et implémenter des pans entiers de fonctionnalités, à charge pour le développeur de reviewer.
- Chaque lot du découpage (section 6) peut être traité comme une itération distincte, avec une revue de code à la fin de chaque lot avant de passer au suivant.
