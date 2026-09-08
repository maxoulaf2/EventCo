# language: fr
Fonctionnalité: Promotion d'un participant en co-organisateur via l'API
  En tant que créateur d'un événement je veux promouvoir un participant en co-organisateur via l'API
  afin de partager la gestion de l'événement

  Scénario: Promotion réussie
    Etant donné une session ouverte via l'API pour "promote-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "promote-guest-api-test@example.com" invité à cet événement via l'API
    Quand je promeus ce participant en co-organisateur via l'API
    Alors la réponse de promotion a le statut 204

  Scénario: Promotion sur un événement inexistant
    Etant donné une session ouverte via l'API pour "promote-missing-event-api-test@example.com"
    Quand je promeus un participant sur un événement inexistant via l'API
    Alors la réponse de promotion a le statut 404

  Scénario: Promotion sans cookie de session
    Quand je promeus un participant sur un événement inexistant via l'API sans cookie de session
    Alors la réponse de promotion a le statut 401

  Scénario: Promotion par un utilisateur qui n'est pas le créateur
    Etant donné une session ouverte via l'API pour "promote-event-creator-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "promote-guest-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "promote-event-other-user-api-test@example.com"
    Quand je promeus ce participant en co-organisateur via l'API
    Alors la réponse de promotion a le statut 403
