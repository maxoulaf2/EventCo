# language: fr
Fonctionnalité: Marquage d'une tâche comme non faite via l'API
  En tant qu'utilisateur connecté je veux rouvrir une tâche déjà marquée comme faite via l'API
  afin de corriger une erreur ou de refaire ce qui reste à faire

  Scénario: Réouverture réussie par le participant assigné
    Etant donné une session ouverte via l'API pour "reopen-task-assignee-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et "reopen-task-assignee-guest-api-test@example.com" invité à cet événement via l'API
    Et cette tâche assignée à ce participant via l'API
    Et une session ouverte via l'API pour "reopen-task-assignee-guest-api-test@example.com"
    Et cette tâche déjà marquée comme faite via l'API
    Quand je marque cette tâche comme non faite via l'API
    Alors la réponse de réouverture a le statut 204

  Scénario: Réouverture réussie par le créateur sur une tâche non assignée
    Etant donné une session ouverte via l'API pour "reopen-task-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et cette tâche déjà marquée comme faite via l'API
    Quand je marque cette tâche comme non faite via l'API
    Alors la réponse de réouverture a le statut 204

  Scénario: Réouverture refusée pour la tâche assignée à un autre participant
    Etant donné une session ouverte via l'API pour "reopen-task-others-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et "reopen-task-others-guest-api-test@example.com" invité à cet événement via l'API
    Et cette tâche assignée à ce participant via l'API
    Et cette tâche déjà marquée comme faite via l'API
    Et "reopen-task-others-other-guest-api-test@example.com" également invité à cet événement via l'API
    Et une session ouverte via l'API pour "reopen-task-others-other-guest-api-test@example.com"
    Quand je marque cette tâche comme non faite via l'API
    Alors la réponse de réouverture a le statut 403

  Scénario: Réouverture sur un événement inexistant
    Etant donné une session ouverte via l'API pour "reopen-task-missing-event-api-test@example.com"
    Quand je marque une tâche comme non faite sur un événement inexistant via l'API
    Alors la réponse de réouverture a le statut 404

  Scénario: Réouverture sans cookie de session
    Quand je marque une tâche comme non faite sur un événement inexistant via l'API sans cookie de session
    Alors la réponse de réouverture a le statut 401
