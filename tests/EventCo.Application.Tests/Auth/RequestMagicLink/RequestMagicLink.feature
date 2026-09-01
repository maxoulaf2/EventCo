# language: fr
Fonctionnalité: Demande de lien de connexion (magic link)
  En tant qu'utilisateur je veux recevoir un lien de connexion par email
  afin de me connecter à EventCo sans mot de passe

  Scénario: Email valide
    Quand je demande un lien de connexion pour "test@example.com"
    Alors la demande est acceptée
    Et un token de connexion est enregistré pour "test@example.com" expirant dans 15 minutes
    Et un email est envoyé à "test@example.com" contenant un lien de vérification

  Scénario: Email vide
    Quand je demande un lien de connexion pour ""
    Alors la demande échoue avec une erreur de validation
    Et aucun email n'est envoyé

  Scénario: Email au format invalide
    Quand je demande un lien de connexion pour "pas-un-email"
    Alors la demande échoue avec une erreur de validation
    Et aucun email n'est envoyé
