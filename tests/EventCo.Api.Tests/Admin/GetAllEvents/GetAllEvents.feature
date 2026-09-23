# language: fr
Fonctionnalité: Consultation de tous les événements par un administrateur via l'API
  En tant qu'administrateur je veux consulter tous les événements existants via l'API
  sans avoir à y participer

  Scénario: Un administrateur consulte tous les événements
    Etant donné une session ouverte via l'API pour "all-events-creator-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une session administrateur ouverte via l'API pour "all-events-admin-api-test@example.com"
    Quand je consulte la liste de tous les événements via l'API
    Alors la réponse de liste de tous les événements a le statut 200
    Et la liste de tous les événements retournée contient cet événement avec 1 participant

  Scénario: Un utilisateur non administrateur ne peut pas consulter tous les événements
    Etant donné une session ouverte via l'API pour "all-events-not-admin-api-test@example.com"
    Quand je consulte la liste de tous les événements via l'API
    Alors la réponse de liste de tous les événements a le statut 403

  Scénario: Consultation sans cookie de session
    Quand je consulte la liste de tous les événements via l'API sans cookie de session
    Alors la réponse de liste de tous les événements a le statut 401
