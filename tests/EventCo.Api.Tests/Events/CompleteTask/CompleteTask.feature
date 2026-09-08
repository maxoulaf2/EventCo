# language: fr
Fonctionnalité: Marquage d'une tâche comme faite via l'API
  En tant qu'utilisateur connecté je veux marquer une tâche comme faite via l'API
  afin de suivre l'avancement des préparatifs

  Scénario: Marquage réussi par le participant assigné
    Etant donné une session ouverte via l'API pour "complete-task-assignee-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et "complete-task-guest-api-test@example.com" invité à cet événement via l'API
    Et cette tâche assignée à ce participant via l'API
    Et une session ouverte via l'API pour "complete-task-guest-api-test@example.com"
    Quand je marque cette tâche comme faite via l'API
    Alors la réponse de marquage a le statut 204

  Scénario: Marquage réussi par le créateur sur une tâche non assignée
    Etant donné une session ouverte via l'API pour "complete-task-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Quand je marque cette tâche comme faite via l'API
    Alors la réponse de marquage a le statut 204

  Scénario: Marquage refusé pour la tâche assignée à un autre participant
    Etant donné une session ouverte via l'API pour "complete-task-others-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et "complete-task-others-guest-api-test@example.com" invité à cet événement via l'API
    Et cette tâche assignée à ce participant via l'API
    Et "complete-task-others-other-guest-api-test@example.com" également invité à cet événement via l'API
    Et une session ouverte via l'API pour "complete-task-others-other-guest-api-test@example.com"
    Quand je marque cette tâche comme faite via l'API
    Alors la réponse de marquage a le statut 403

  Scénario: Marquage sur un événement inexistant
    Etant donné une session ouverte via l'API pour "complete-task-missing-event-api-test@example.com"
    Quand je marque une tâche comme faite sur un événement inexistant via l'API
    Alors la réponse de marquage a le statut 404

  Scénario: Marquage sans cookie de session
    Quand je marque une tâche comme faite sur un événement inexistant via l'API sans cookie de session
    Alors la réponse de marquage a le statut 401
