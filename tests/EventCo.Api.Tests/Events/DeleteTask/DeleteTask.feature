# language: fr
Fonctionnalité: Suppression d'une tâche via l'API
  En tant qu'utilisateur connecté je veux supprimer une tâche via l'API
  afin de retirer une tâche devenue inutile de la liste des préparatifs

  Scénario: Suppression réussie par le participant qui a créé la tâche
    Etant donné une session ouverte via l'API pour "delete-task-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "delete-task-creator-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "delete-task-creator-api-test@example.com"
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Quand je supprime cette tâche via l'API
    Alors la réponse de suppression de tâche a le statut 204

  Scénario: Suppression réussie par le créateur de l'événement sur une tâche créée par un autre participant
    Etant donné une session ouverte via l'API pour "delete-task-organizer2-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "delete-task-creator2-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "delete-task-creator2-api-test@example.com"
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et une session ouverte via l'API pour "delete-task-organizer2-api-test@example.com"
    Quand je supprime cette tâche via l'API
    Alors la réponse de suppression de tâche a le statut 204

  Scénario: Suppression refusée pour un participant qui n'a pas créé la tâche
    Etant donné une session ouverte via l'API pour "delete-task-organizer3-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "delete-task-creator3-api-test@example.com" invité à cet événement via l'API
    Et "delete-task-other3-api-test@example.com" également invité à cet événement via l'API
    Et une session ouverte via l'API pour "delete-task-creator3-api-test@example.com"
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et une session ouverte via l'API pour "delete-task-other3-api-test@example.com"
    Quand je supprime cette tâche via l'API
    Alors la réponse de suppression de tâche a le statut 403

  Scénario: Suppression sur un événement inexistant
    Etant donné une session ouverte via l'API pour "delete-task-missing-event-api-test@example.com"
    Quand je supprime une tâche sur un événement inexistant via l'API
    Alors la réponse de suppression de tâche a le statut 404

  Scénario: Suppression sans cookie de session
    Quand je supprime une tâche sur un événement inexistant via l'API sans cookie de session
    Alors la réponse de suppression de tâche a le statut 401
