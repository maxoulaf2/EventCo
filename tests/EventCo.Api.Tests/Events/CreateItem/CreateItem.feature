# language: fr
Fonctionnalité: Création d'un article via l'API
  En tant qu'utilisateur connecté je veux ajouter ce que j'apporte à un événement via l'API,
  et en tant qu'organisateur ce qu'il faudrait apporter, afin de répartir les préparatifs

  Scénario: Création d'un article à prendre par le créateur
    Etant donné une session ouverte via l'API pour "create-item-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand j'ajoute l'article à prendre "Bûche au chocolat" de quantité "1" à cet événement via l'API
    Alors la réponse de création d'article a le statut 201
    Et l'article créé via l'API est de nature "ToBring"
    Et l'article créé via l'API n'est attribué à personne

  Scénario: Création sur un événement inexistant
    Etant donné une session ouverte via l'API pour "create-item-missing-event-api-test@example.com"
    Quand j'ajoute l'article que j'apporte "Bûche au chocolat" de quantité "1" à un événement inexistant via l'API
    Alors la réponse de création d'article a le statut 404

  Scénario: Création sans cookie de session
    Quand j'ajoute l'article que j'apporte "Bûche au chocolat" de quantité "1" à un événement inexistant via l'API sans cookie de session
    Alors la réponse de création d'article a le statut 401

  Scénario: Création d'un article apporté par un participant simple invité (non organisateur)
    Etant donné une session ouverte via l'API pour "create-item-invited-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "create-item-invited-participant-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "create-item-invited-participant-api-test@example.com"
    Quand j'ajoute l'article que j'apporte "Bûche au chocolat" de quantité "1" à cet événement via l'API
    Alors la réponse de création d'article a le statut 201
    Et l'article créé via l'API est de nature "Contribution"
    Et l'article créé via l'API est attribué à ce participant

  Scénario: Création d'un article à prendre refusée pour un participant simple
    Etant donné une session ouverte via l'API pour "create-item-to-bring-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "create-item-to-bring-participant-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "create-item-to-bring-participant-api-test@example.com"
    Quand j'ajoute l'article à prendre "Bûche au chocolat" de quantité "1" à cet événement via l'API
    Alors la réponse de création d'article a le statut 403

  Scénario: Création d'un article de nature inconnue
    Etant donné une session ouverte via l'API pour "create-item-unknown-kind-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand j'ajoute l'article de nature "Autre" "Bûche au chocolat" de quantité "1" à cet événement via l'API
    Alors la réponse de création d'article a le statut 400

  Scénario: Création par un utilisateur qui ne participe pas à l'événement
    Etant donné une session ouverte via l'API pour "create-item-event-creator-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une session ouverte via l'API pour "create-item-event-other-user-api-test@example.com"
    Quand j'ajoute l'article que j'apporte "Bûche au chocolat" de quantité "1" à cet événement via l'API
    Alors la réponse de création d'article a le statut 403
