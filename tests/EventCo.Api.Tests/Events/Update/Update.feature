# language: fr
Fonctionnalité: Modification d'un événement via l'API
  En tant qu'utilisateur connecté je veux modifier un événement via l'API
  afin d'en corriger les informations

  Scénario: Modification d'un événement existant avec une session valide
    Etant donné une session ouverte via l'API pour "update-event-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand je modifie cet événement via l'API avec le titre "Réveillon de Noël" au lieu "Chez Bob"
    Alors la réponse de modification d'événement a le statut 200
    Et l'événement modifié retourné a pour titre "Réveillon de Noël"

  Scénario: Modification d'un événement inexistant
    Etant donné une session ouverte via l'API pour "update-missing-event-api-test@example.com"
    Quand je modifie un événement inexistant via l'API avec le titre "Réveillon de Noël" au lieu "Chez Bob"
    Alors la réponse de modification d'événement a le statut 404

  Scénario: Modification sans cookie de session
    Quand je modifie un événement inexistant via l'API avec le titre "Réveillon de Noël" au lieu "Chez Bob" sans cookie de session
    Alors la réponse de modification d'événement a le statut 401
