# language: fr
Fonctionnalité: Création d'un article via l'API
  En tant qu'utilisateur connecté je veux ajouter un article à un événement via l'API
  afin de répartir ce qu'il reste à faire

  Scénario: Création réussie
    Etant donné une session ouverte via l'API pour "create-item-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand j'ajoute l'article "Bûche au chocolat" de quantité "1" à cet événement via l'API
    Alors la réponse de création d'article a le statut 201

  Scénario: Création sur un événement inexistant
    Etant donné une session ouverte via l'API pour "create-item-missing-event-api-test@example.com"
    Quand j'ajoute l'article "Bûche au chocolat" de quantité "1" à un événement inexistant via l'API
    Alors la réponse de création d'article a le statut 404

  Scénario: Création sans cookie de session
    Quand j'ajoute l'article "Bûche au chocolat" de quantité "1" à un événement inexistant via l'API sans cookie de session
    Alors la réponse de création d'article a le statut 401

  Scénario: Création par un participant simple invité (non organisateur)
    Etant donné une session ouverte via l'API pour "create-item-invited-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "create-item-invited-participant-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "create-item-invited-participant-api-test@example.com"
    Quand j'ajoute l'article "Bûche au chocolat" de quantité "1" à cet événement via l'API
    Alors la réponse de création d'article a le statut 201

  Scénario: Création par un utilisateur qui ne participe pas à l'événement
    Etant donné une session ouverte via l'API pour "create-item-event-creator-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une session ouverte via l'API pour "create-item-event-other-user-api-test@example.com"
    Quand j'ajoute l'article "Bûche au chocolat" de quantité "1" à cet événement via l'API
    Alors la réponse de création d'article a le statut 403
