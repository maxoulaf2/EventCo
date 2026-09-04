# language: fr
Fonctionnalité: Consultation d'un événement via l'API
  En tant qu'utilisateur connecté je veux consulter un événement via l'API
  afin d'en voir les détails

  Scénario: Consultation d'un événement existant avec une session valide
    Etant donné une session ouverte via l'API pour "get-event-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand je consulte cet événement via l'API
    Alors la réponse de consultation d'événement a le statut 200
    Et l'événement consulté retourné a pour titre "Repas de Noël"

  Scénario: Consultation d'un événement inexistant
    Etant donné une session ouverte via l'API pour "get-missing-event-api-test@example.com"
    Quand je consulte un événement inexistant via l'API
    Alors la réponse de consultation d'événement a le statut 404

  Scénario: Consultation sans cookie de session
    Quand je consulte un événement inexistant via l'API sans cookie de session
    Alors la réponse de consultation d'événement a le statut 401
