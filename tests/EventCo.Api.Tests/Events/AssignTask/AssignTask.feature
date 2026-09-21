# language: fr
Fonctionnalité: Assignation d'une tâche à un participant via l'API
  En tant qu'utilisateur connecté je veux assigner une tâche à un participant via l'API
  afin de savoir qui s'occupe de quoi

  Scénario: Assignation réussie par le créateur à un participant
    Etant donné une session ouverte via l'API pour "assign-task-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et "assign-task-guest-api-test@example.com" invité à cet événement via l'API
    Quand j'assigne cette tâche à ce participant via l'API
    Alors la réponse d'assignation a le statut 204

  Scénario: Auto-assignation réussie par un participant simple
    Etant donné une session ouverte via l'API pour "assign-task-self-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et "assign-task-self-guest-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "assign-task-self-guest-api-test@example.com"
    Quand j'assigne cette tâche à ce participant via l'API
    Alors la réponse d'assignation a le statut 204

  Scénario: Assignation par un participant simple à un autre participant
    Etant donné une session ouverte via l'API pour "assign-task-others-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et "assign-task-others-guest-api-test@example.com" invité à cet événement via l'API
    Et "assign-task-others-other-guest-api-test@example.com" également invité à cet événement via l'API
    Et une session ouverte via l'API pour "assign-task-others-guest-api-test@example.com"
    Quand j'assigne cette tâche à l'autre participant via l'API
    Alors la réponse d'assignation a le statut 403

  Scénario: Assignation à un utilisateur qui n'est pas participant
    Etant donné une session ouverte via l'API pour "assign-task-invalid-target-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Quand j'assigne cette tâche à un utilisateur qui n'est pas participant via l'API
    Alors la réponse d'assignation a le statut 400

  Scénario: Assignation sur un événement inexistant
    Etant donné une session ouverte via l'API pour "assign-task-missing-event-api-test@example.com"
    Quand j'assigne une tâche à un événement inexistant via l'API
    Alors la réponse d'assignation a le statut 404

  Scénario: Assignation sans cookie de session
    Quand j'assigne une tâche à un événement inexistant via l'API sans cookie de session
    Alors la réponse d'assignation a le statut 401

  Scénario: Assignation par un utilisateur qui ne participe pas à l'événement
    Etant donné une session ouverte via l'API pour "assign-task-organizer5-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et une session ouverte via l'API pour "assign-task-non-participant5-api-test@example.com"
    Quand j'assigne cette tâche à un utilisateur qui n'est pas participant via l'API
    Alors la réponse d'assignation a le statut 403
