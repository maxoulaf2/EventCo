# language: fr
Fonctionnalité: Assignation d'un article à un participant via l'API
  En tant qu'utilisateur connecté je veux assigner un article à un participant via l'API
  afin de savoir qui s'occupe de quoi

  Scénario: Assignation réussie par le créateur à un participant
    Etant donné une session ouverte via l'API pour "assign-item-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un article à prendre "Bûche au chocolat" ajouté à cet événement via l'API
    Et "assign-item-guest-api-test@example.com" invité à cet événement via l'API
    Quand j'assigne cet article à ce participant via l'API
    Alors la réponse d'assignation a le statut 204

  Scénario: Auto-assignation réussie par un participant simple
    Etant donné une session ouverte via l'API pour "assign-item-self-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un article à prendre "Bûche au chocolat" ajouté à cet événement via l'API
    Et "assign-item-self-guest-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "assign-item-self-guest-api-test@example.com"
    Quand j'assigne cet article à ce participant via l'API
    Alors la réponse d'assignation a le statut 204

  Scénario: Assignation par un participant simple à un autre participant
    Etant donné une session ouverte via l'API pour "assign-item-others-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un article à prendre "Bûche au chocolat" ajouté à cet événement via l'API
    Et "assign-item-others-guest-api-test@example.com" invité à cet événement via l'API
    Et "assign-item-others-other-guest-api-test@example.com" également invité à cet événement via l'API
    Et une session ouverte via l'API pour "assign-item-others-guest-api-test@example.com"
    Quand j'assigne cet article à l'autre participant via l'API
    Alors la réponse d'assignation a le statut 403

  Scénario: Assignation à un utilisateur qui n'est pas participant
    Etant donné une session ouverte via l'API pour "assign-item-invalid-target-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un article à prendre "Bûche au chocolat" ajouté à cet événement via l'API
    Quand j'assigne cet article à un utilisateur qui n'est pas participant via l'API
    Alors la réponse d'assignation a le statut 400

  Scénario: Assignation sur un événement inexistant
    Etant donné une session ouverte via l'API pour "assign-item-missing-event-api-test@example.com"
    Quand j'assigne un article à un événement inexistant via l'API
    Alors la réponse d'assignation a le statut 404

  Scénario: Assignation sans cookie de session
    Quand j'assigne un article à un événement inexistant via l'API sans cookie de session
    Alors la réponse d'assignation a le statut 401

  Scénario: Assignation par un utilisateur qui ne participe pas à l'événement
    Etant donné une session ouverte via l'API pour "assign-item-organizer5-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un article à prendre "Bûche au chocolat" ajouté à cet événement via l'API
    Et une session ouverte via l'API pour "assign-item-non-participant5-api-test@example.com"
    Quand j'assigne cet article à un utilisateur qui n'est pas participant via l'API
    Alors la réponse d'assignation a le statut 403

  Scénario: Réattribution refusée au créateur pour un article apporté par un participant
    Etant donné une session ouverte via l'API pour "assign-item-contribution-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "assign-item-contribution-guest-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "assign-item-contribution-guest-api-test@example.com"
    Et un article apporté "Bûche au chocolat" ajouté à cet événement via l'API
    Et une session ouverte via l'API pour "assign-item-contribution-organizer-api-test@example.com"
    Quand j'assigne cet article à ce participant via l'API
    Alors la réponse d'assignation a le statut 403
