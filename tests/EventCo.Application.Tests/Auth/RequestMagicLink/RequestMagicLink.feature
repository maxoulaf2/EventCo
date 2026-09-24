# language: fr
Fonctionnalité: Demande de code de connexion
  En tant qu'utilisateur je veux recevoir un code de connexion par email
  afin de me connecter à EventCo sans mot de passe

  Scénario: Email valide
    Quand je demande un code de connexion pour "test@example.com"
    Alors la demande est acceptée
    Et un code de connexion est enregistré pour "test@example.com" expirant dans 15 minutes
    Et un email est envoyé à "test@example.com" contenant un code à 6 chiffres
    Et le code envoyé n'est pas stocké en clair

  Scénario: Email vide
    Quand je demande un code de connexion pour ""
    Alors la demande échoue avec une erreur de validation
    Et aucun email n'est envoyé

  Scénario: Email au format invalide
    Quand je demande un code de connexion pour "pas-un-email"
    Alors la demande échoue avec une erreur de validation
    Et aucun email n'est envoyé

  Scénario: Trop de demandes pour le même email dans la même fenêtre
    Quand je demande un code de connexion pour "spam@example.com"
    Et je demande un code de connexion pour "spam@example.com"
    Et je demande un code de connexion pour "spam@example.com"
    Et je demande un code de connexion pour "spam@example.com"
    Et je demande un code de connexion pour "spam@example.com"
    Et je demande un code de connexion pour "spam@example.com"
    Alors la demande échoue avec une erreur de trop de requêtes
    Et exactement 5 emails ont été envoyés à "spam@example.com"
