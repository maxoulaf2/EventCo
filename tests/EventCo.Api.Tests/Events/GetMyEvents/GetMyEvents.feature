# language: fr
Fonctionnalité: Consultation de mes événements via l'API
  En tant qu'utilisateur connecté je veux consulter la liste de mes événements via l'API
  afin de les retrouver depuis le frontend

  Scénario: Liste des événements après en avoir créé un
    Etant donné une session ouverte via l'API pour "list-events-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand je consulte la liste de mes événements via l'API
    Alors la réponse de liste d'événements a le statut 200
    Et ma liste d'événements retournée contient "Repas de Noël"

  Scénario: Consultation sans cookie de session
    Quand je consulte la liste de mes événements via l'API sans cookie de session
    Alors la réponse de liste d'événements a le statut 401
