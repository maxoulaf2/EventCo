# language: fr
Fonctionnalité: Rétrogradation d'un co-organisateur en participant via l'API
  En tant que créateur d'un événement je veux rétrograder un co-organisateur en simple participant via l'API
  afin de reprendre la main sur la gestion de l'événement

  Scénario: Rétrogradation réussie
    Etant donné une session ouverte via l'API pour "demote-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "demote-guest-api-test@example.com" invité à cet événement via l'API
    Et ce participant est déjà promu co-organisateur via l'API
    Quand je rétrograde ce co-organisateur en participant via l'API
    Alors la réponse de rétrogradation a le statut 204

  Scénario: Rétrogradation sur un événement inexistant
    Etant donné une session ouverte via l'API pour "demote-missing-event-api-test@example.com"
    Quand je rétrograde un co-organisateur sur un événement inexistant via l'API
    Alors la réponse de rétrogradation a le statut 404

  Scénario: Rétrogradation sans cookie de session
    Quand je rétrograde un co-organisateur sur un événement inexistant via l'API sans cookie de session
    Alors la réponse de rétrogradation a le statut 401

  Scénario: Rétrogradation par un utilisateur qui n'est pas le créateur
    Etant donné une session ouverte via l'API pour "demote-event-creator-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "demote-guest-api-test@example.com" invité à cet événement via l'API
    Et ce participant est déjà promu co-organisateur via l'API
    Et une session ouverte via l'API pour "demote-event-other-user-api-test@example.com"
    Quand je rétrograde ce co-organisateur en participant via l'API
    Alors la réponse de rétrogradation a le statut 403

  Scénario: Rétrogradation du créateur lui-même
    Etant donné une session ouverte via l'API pour "demote-event-self-creator-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand je rétrograde le créateur de l'événement via l'API
    Alors la réponse de rétrogradation a le statut 400

  Scénario: Rétrogradation d'un utilisateur qui ne participe pas à l'événement
    Etant donné une session ouverte via l'API pour "demote-not-participant-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand je rétrograde un utilisateur qui ne participe pas à cet événement via l'API
    Alors la réponse de rétrogradation a le statut 400
