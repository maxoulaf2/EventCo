# language: fr
Fonctionnalité: Création d'un événement via l'API
  En tant qu'utilisateur connecté je veux créer un événement via l'API
  afin de co-organiser un événement de groupe avec mes amis

  Scénario: Création avec des données valides et une session valide
    Etant donné une session ouverte via l'API pour "create-event-api-test@example.com"
    Quand je crée l'événement "Repas de Noël"
    Alors la réponse de création d'événement a le statut 201
    Et l'événement créé retourné a pour titre "Repas de Noël"

  Scénario: Création sans cookie de session
    Quand je crée l'événement "Repas de Noël"
    Alors la réponse de création d'événement a le statut 401
