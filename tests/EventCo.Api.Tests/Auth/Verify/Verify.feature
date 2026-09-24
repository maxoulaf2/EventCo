# language: fr
Fonctionnalité: Validation du code de connexion via l'API
  En tant qu'utilisateur je veux appeler l'API EventCo
  afin d'ouvrir une session à partir du code de connexion reçu par email

  Scénario: Code valide
    Quand un code de connexion est demandé via l'API pour "verify-api-test@example.com"
    Et je valide le code de connexion reçu via l'API
    Alors la réponse de vérification a le statut 200
    Et un cookie de session httpOnly est présent dans la réponse
    Et un compte est persisté en base pour "verify-api-test@example.com"

  Scénario: Code erroné
    Quand un code de connexion est demandé via l'API pour "verify-api-errone@example.com"
    Et je valide un code erroné via l'API
    Alors la réponse de vérification a le statut 400
    Et aucun cookie de session n'est présent dans la réponse
    Et 1 essai erroné est persisté en base pour "verify-api-errone@example.com"

  Scénario: Code au format invalide
    Quand je valide via l'API le code "abc" pour l'email "verify-api-format@example.com"
    Alors la réponse de vérification a le statut 400
