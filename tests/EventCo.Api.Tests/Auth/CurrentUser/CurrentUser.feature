# language: fr
Fonctionnalité: Résolution de l'utilisateur courant via l'API
  En tant qu'utilisateur je veux appeler l'API EventCo
  afin de connaître mon identité à partir de mon cookie de session

  Scénario: Cookie de session valide
    Etant donné une session ouverte via l'API pour "current-user-api-test@example.com"
    Quand j'appelle "/api/auth/me" avec le cookie de session obtenu
    Alors la réponse de l'utilisateur courant a le statut 200
    Et l'utilisateur courant retourné a pour email "current-user-api-test@example.com"

  Scénario: Aucun cookie de session
    Quand j'appelle "/api/auth/me" sans cookie de session
    Alors la réponse de l'utilisateur courant a le statut 401

  Scénario: Cookie de session invalide
    Quand j'appelle "/api/auth/me" avec le cookie de session invalide "valeur-falsifiee"
    Alors la réponse de l'utilisateur courant a le statut 401
