# language: fr
Fonctionnalité: Demande de lien de connexion (magic link) via l'API
  En tant qu'utilisateur je veux appeler l'API EventCo
  afin de recevoir un lien de connexion par email

  Scénario: Email valide
    Quand j'envoie une requête POST à "/api/auth/request-link" avec l'email "api-test@example.com"
    Alors la réponse a le statut 202
    Et un token de connexion est persisté en base pour "api-test@example.com"
