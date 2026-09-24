# language: fr
Fonctionnalité: Consultation des articles d'un événement via l'API
  En tant qu'utilisateur connecté je veux consulter la liste des articles d'un événement via l'API
  afin de savoir ce qu'il reste à faire

  Scénario: Consultation des articles d'un événement avec une session valide
    Etant donné une session ouverte via l'API pour "get-event-items-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un article "Bûche au chocolat" ajouté à cet événement via l'API
    Quand je consulte les articles de cet événement via l'API
    Alors la réponse de consultation des articles a le statut 200
    Et les articles consultés retournés contiennent 1 article
    Et les articles consultés retournés contiennent un article "Bûche au chocolat"

  Scénario: Consultation des articles d'un événement inexistant
    Etant donné une session ouverte via l'API pour "get-event-items-missing-event-api-test@example.com"
    Quand je consulte les articles d'un événement inexistant via l'API
    Alors la réponse de consultation des articles a le statut 404

  Scénario: Consultation des articles sans cookie de session
    Quand je consulte les articles d'un événement inexistant via l'API sans cookie de session
    Alors la réponse de consultation des articles a le statut 401

  Scénario: Consultation des articles par un utilisateur qui n'est pas participant
    Etant donné une session ouverte via l'API pour "get-event-items-creator-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une session ouverte via l'API pour "get-event-items-other-user-api-test@example.com"
    Quand je consulte les articles de cet événement via l'API
    Alors la réponse de consultation des articles a le statut 403
