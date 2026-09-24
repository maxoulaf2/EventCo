# language: fr
Fonctionnalité: Création d'une tâche via l'API
  En tant qu'utilisateur connecté je veux ajouter une tâche à un événement via l'API
  afin de répartir ce qu'il reste à faire

  Scénario: Création réussie
    Etant donné une session ouverte via l'API pour "create-task-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand j'ajoute la tâche "Bûche au chocolat" de quantité "1" à cet événement via l'API
    Alors la réponse de création de tâche a le statut 201

  Scénario: Création sur un événement inexistant
    Etant donné une session ouverte via l'API pour "create-task-missing-event-api-test@example.com"
    Quand j'ajoute la tâche "Bûche au chocolat" de quantité "1" à un événement inexistant via l'API
    Alors la réponse de création de tâche a le statut 404

  Scénario: Création sans cookie de session
    Quand j'ajoute la tâche "Bûche au chocolat" de quantité "1" à un événement inexistant via l'API sans cookie de session
    Alors la réponse de création de tâche a le statut 401

  Scénario: Création par un participant simple invité (non organisateur)
    Etant donné une session ouverte via l'API pour "create-task-invited-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "create-task-invited-participant-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "create-task-invited-participant-api-test@example.com"
    Quand j'ajoute la tâche "Bûche au chocolat" de quantité "1" à cet événement via l'API
    Alors la réponse de création de tâche a le statut 201

  Scénario: Création par un utilisateur qui ne participe pas à l'événement
    Etant donné une session ouverte via l'API pour "create-task-event-creator-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une session ouverte via l'API pour "create-task-event-other-user-api-test@example.com"
    Quand j'ajoute la tâche "Bûche au chocolat" de quantité "1" à cet événement via l'API
    Alors la réponse de création de tâche a le statut 403
