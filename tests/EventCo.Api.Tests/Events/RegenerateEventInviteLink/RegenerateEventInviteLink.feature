# language: fr
Fonctionnalité: Régénération du lien d'invitation via l'API
  En tant qu'organisateur je veux régénérer le lien d'invitation d'un événement via l'API
  afin d'invalider l'ancien lien

  Scénario: Le créateur régénère le lien
    Etant donné une session ouverte via l'API pour "regenerate-link-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand je régénère le lien d'invitation de cet événement via l'API
    Alors la réponse de régénération a le statut 200
    Et le nouveau lien d'invitation diffère de l'ancien

  Scénario: Un utilisateur qui n'est ni créateur ni co-organisateur ne peut pas régénérer le lien
    Etant donné une session ouverte via l'API pour "regenerate-link-creator-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une session ouverte via l'API pour "regenerate-link-other-user-api-test@example.com"
    Quand je régénère le lien d'invitation de cet événement via l'API
    Alors la réponse de régénération a le statut 403

  Scénario: Régénération sans cookie de session
    Etant donné une session ouverte via l'API pour "regenerate-link-anon-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand je régénère le lien d'invitation de cet événement via l'API sans cookie de session
    Alors la réponse de régénération a le statut 401

  Scénario: Régénération sur un événement inexistant
    Etant donné une session ouverte via l'API pour "regenerate-link-missing-event-api-test@example.com"
    Quand je régénère le lien d'invitation d'un événement inexistant via l'API
    Alors la réponse de régénération a le statut 404
