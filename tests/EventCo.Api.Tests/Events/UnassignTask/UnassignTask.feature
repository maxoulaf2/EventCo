# language: fr
Fonctionnalité: Désassignation d'une tâche via l'API
  En tant qu'utilisateur connecté je veux pouvoir désassigner une tâche via l'API
  afin de la rendre disponible pour quelqu'un d'autre si je ne peux plus m'en occuper

  Scénario: Désassignation réussie par le participant assigné
    Etant donné une session ouverte via l'API pour "unassign-task-assignee-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et "unassign-task-assignee-guest-api-test@example.com" invité à cet événement via l'API
    Et cette tâche assignée à ce participant via l'API
    Et une session ouverte via l'API pour "unassign-task-assignee-guest-api-test@example.com"
    Quand je désassigne cette tâche via l'API
    Alors la réponse de désassignation a le statut 204

  Scénario: Désassignation réussie par le créateur pour une tâche assignée à un autre participant
    Etant donné une session ouverte via l'API pour "unassign-task-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et "unassign-task-guest-api-test@example.com" invité à cet événement via l'API
    Et cette tâche assignée à ce participant via l'API
    Quand je désassigne cette tâche via l'API
    Alors la réponse de désassignation a le statut 204

  Scénario: Désassignation refusée pour la tâche assignée à un autre participant
    Etant donné une session ouverte via l'API pour "unassign-task-others-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et "unassign-task-others-guest-api-test@example.com" invité à cet événement via l'API
    Et cette tâche assignée à ce participant via l'API
    Et "unassign-task-others-other-guest-api-test@example.com" également invité à cet événement via l'API
    Et une session ouverte via l'API pour "unassign-task-others-other-guest-api-test@example.com"
    Quand je désassigne cette tâche via l'API
    Alors la réponse de désassignation a le statut 403

  Scénario: Désassignation sur un événement inexistant
    Etant donné une session ouverte via l'API pour "unassign-task-missing-event-api-test@example.com"
    Quand je désassigne une tâche sur un événement inexistant via l'API
    Alors la réponse de désassignation a le statut 404

  Scénario: Désassignation sans cookie de session
    Quand je désassigne une tâche sur un événement inexistant via l'API sans cookie de session
    Alors la réponse de désassignation a le statut 401
