# language: fr
Fonctionnalité: Validation du lien de connexion (magic link) via l'API
  En tant qu'utilisateur je veux appeler l'API EventCo
  afin d'ouvrir une session à partir de mon lien de connexion

  Scénario: Token valide
    Quand un lien de connexion est demandé via l'API pour "verify-api-test@example.com"
    Et je valide le lien de connexion reçu via l'API
    Alors la réponse de vérification a le statut 200
    Et un cookie de session httpOnly est présent dans la réponse
    Et un compte est persisté en base pour "verify-api-test@example.com"

  Scénario: Token invalide
    Quand j'envoie une requête POST à "/api/auth/verify" avec le token "token-inexistant"
    Alors la réponse de vérification a le statut 400
