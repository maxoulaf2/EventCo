# language: fr
Fonctionnalité: Demande de code de connexion via l'API
  En tant qu'utilisateur je veux appeler l'API EventCo
  afin de recevoir un code de connexion par email

  Scénario: Email valide
    Quand j'envoie une requête POST à "/api/auth/request-link" avec l'email "api-test@example.com"
    Alors la réponse a le statut 202
    Et un code de connexion est persisté en base pour "api-test@example.com"

  Scénario: Trop de demandes pour le même email
    Quand j'envoie 6 requêtes POST à "/api/auth/request-link" avec l'email "api-spam@example.com"
    Alors la réponse a le statut 429

  Scénario: Email invalide
    Quand j'envoie une requête POST à "/api/auth/request-link" avec l'email "pas-un-email"
    Alors la réponse a le statut 400
