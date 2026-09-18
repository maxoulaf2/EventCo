# language: fr
Fonctionnalité: Indication de participation à un événement via l'API
  En tant que participant d'un événement je veux indiquer si je viens via l'API
  afin que les autres participants sachent qui est présent

  Scénario: Indication réussie par le participant lui-même
    Etant donné une session ouverte via l'API pour "participation-status-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "participation-status-guest-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "participation-status-guest-api-test@example.com"
    Quand j'indique le statut de participation "Attending" via l'API
    Alors la réponse de changement de statut a le statut 204

  Scénario: Le créateur ne peut pas modifier son propre statut de participation
    Etant donné une session ouverte via l'API pour "participation-status-creator-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand j'indique le statut de participation "NotAttending" via l'API
    Alors la réponse de changement de statut a le statut 400

  Scénario: Indication par un utilisateur qui ne participe pas à l'événement
    Etant donné une session ouverte via l'API pour "participation-status-organizer2-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une session ouverte via l'API pour "participation-status-outsider-api-test@example.com"
    Quand j'indique le statut de participation "Attending" via l'API
    Alors la réponse de changement de statut a le statut 400

  Scénario: Statut de participation invalide
    Etant donné une session ouverte via l'API pour "participation-status-invalid-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand j'indique le statut de participation "Peut-être" via l'API
    Alors la réponse de changement de statut a le statut 400

  Scénario: Indication sur un événement inexistant
    Etant donné une session ouverte via l'API pour "participation-status-missing-event-api-test@example.com"
    Quand j'indique le statut de participation "Attending" sur un événement inexistant via l'API
    Alors la réponse de changement de statut a le statut 404

  Scénario: Indication sans cookie de session
    Quand j'indique le statut de participation "Attending" sur un événement inexistant via l'API sans cookie de session
    Alors la réponse de changement de statut a le statut 401
