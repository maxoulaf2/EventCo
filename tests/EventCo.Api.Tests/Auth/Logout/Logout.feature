# language: fr
Fonctionnalité: Déconnexion via l'API
  En tant qu'utilisateur connecté je veux appeler l'API EventCo
  afin de mettre fin à ma session et supprimer mon cookie de session

  Scénario: Déconnexion avec une session valide
    Etant donné une session ouverte via l'API pour "logout-api-test@example.com"
    Quand je me déconnecte via l'API avec le cookie de session obtenu
    Alors la réponse de déconnexion a le statut 204
    Et le cookie de session est supprimé dans la réponse

  Scénario: Déconnexion sans cookie de session
    Quand je me déconnecte via l'API sans cookie de session
    Alors la réponse de déconnexion a le statut 401
