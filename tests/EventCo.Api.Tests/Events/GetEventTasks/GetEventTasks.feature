# language: fr
Fonctionnalité: Consultation des tâches d'un événement via l'API
  En tant qu'utilisateur connecté je veux consulter la liste des tâches d'un événement via l'API
  afin de savoir ce qu'il reste à faire

  Scénario: Consultation des tâches d'un événement avec une session valide
    Etant donné une session ouverte via l'API pour "get-event-tasks-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Quand je consulte les tâches de cet événement via l'API
    Alors la réponse de consultation des tâches a le statut 200
    Et les tâches consultées retournées contiennent 1 tâche
    Et les tâches consultées retournées contiennent une tâche "Bûche au chocolat" non faite

  Scénario: Consultation des tâches d'un événement inexistant
    Etant donné une session ouverte via l'API pour "get-event-tasks-missing-event-api-test@example.com"
    Quand je consulte les tâches d'un événement inexistant via l'API
    Alors la réponse de consultation des tâches a le statut 404

  Scénario: Consultation des tâches sans cookie de session
    Quand je consulte les tâches d'un événement inexistant via l'API sans cookie de session
    Alors la réponse de consultation des tâches a le statut 401

  Scénario: Consultation des tâches par un utilisateur qui n'est pas participant
    Etant donné une session ouverte via l'API pour "get-event-tasks-creator-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une session ouverte via l'API pour "get-event-tasks-other-user-api-test@example.com"
    Quand je consulte les tâches de cet événement via l'API
    Alors la réponse de consultation des tâches a le statut 403
