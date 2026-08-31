# CLAUDE.md

Ce fichier est lu automatiquement par Claude Code au démarrage d'une session dans ce repo. Il centralise le contexte du projet et les règles de fonctionnement.

## Contexte du projet

EventCo est un SaaS de planification d'événements de groupe (repas, anniversaires, week-ends entre amis...) permettant de co-organiser un événement à plusieurs : invitations, répartition des tâches/courses, suivi en temps réel.

Projet personnel de montée en compétences, mené selon des standards professionnels (architecture propre, tests, CI/CD).

## Documents de référence — à consulter systématiquement

| Fichier | Contenu | Quand le consulter |
|---|---|---|
| `docs/cadrage-projet-eventco.md` | Spécifications fonctionnelles, stack technique, modèle de données, priorisation MoSCoW | Avant toute nouvelle fonctionnalité, pour comprendre le besoin métier |
| `docs/conventions-code.md` | Architecture Clean Architecture/DDD (backend), conventions React (frontend), règles Git/API/tests | Avant d'écrire du code, pour respecter la structure et le style attendus |
| `docs/suivi-todo.md` | Backlog détaillé par lot, avec cases à cocher et journal d'avancement | En début et fin de chaque tâche — **c'est la source de vérité de l'avancement** |

## Règles de fonctionnement

1. **Toujours commencer par lire `docs/suivi-todo.md`** pour identifier la prochaine tâche non cochée du lot en cours. Ne pas travailler sur une tâche d'un lot ultérieur si le lot en cours n'est pas terminé (Must have), sauf demande explicite de l'utilisateur.
2. **Respecter scrupuleusement `docs/conventions-code.md`** : structure de dossiers (Domain/Application/Infrastructure/Api côté backend, feature-based côté React), conventions de nommage, principes DDD (agrégats, use cases explicites, pas de setters publics non contrôlés).
3. **Après chaque tâche terminée** (code fonctionnel, testé, cohérent avec les conventions) :
   - Cocher la case correspondante dans `docs/suivi-todo.md`
   - Ajouter une ligne dans le "Journal d'avancement" du même fichier (date, lot, description, fichiers principaux)
4. **En cas de tâche bloquée**, ne pas la cocher et ajouter une note `> Bloqué : raison` juste en dessous dans `docs/suivi-todo.md`.
5. **En cas de tâche manquante identifiée en cours de route**, l'ajouter dans le lot concerné de `docs/suivi-todo.md` avant de la traiter, plutôt que de l'exécuter sans traçabilité.
6. **Le développeur review chaque lot** avant de passer au suivant : signaler clairement quand toutes les tâches Must have d'un lot sont cochées, pour déclencher cette review.
7. **En cas de doute d'architecture non couvert** par `docs/conventions-code.md`, proposer une solution cohérente avec les principes déjà établis (Clean Architecture, DDD, mobile-first) plutôt que d'improviser un pattern différent — et le signaler explicitement pour validation.

## Stack technique (résumé — détails dans `docs/cadrage-projet-eventco.md`)

- **Backend** : ASP.NET Core (C#), Clean Architecture + DDD, PostgreSQL + EF Core, SignalR pour le temps réel
- **Frontend** : React + TypeScript + Tailwind CSS (mobile-first), React Query, PWA
- **Auth** : passwordless (magic link par email), cookie de session httpOnly
- **Tests** : xUnit (Domain/Application/Api)

## Commandes utiles

> À compléter au fur et à mesure de la mise en place du projet (build, run, tests, migrations EF Core, etc.)

```bash
# Backend
# dotnet build
# dotnet run --project src/EventCo.Api
# dotnet ef migrations add <NomMigration> --project src/EventCo.Infrastructure --startup-project src/EventCo.Api
# dotnet test

# Frontend
# npm install
# npm run dev
# npm run build
```
