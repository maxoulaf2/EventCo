# language: fr
Fonctionnalité: Saisie du code de connexion
  En tant qu'utilisateur je veux saisir le code reçu par email sur la page de confirmation
  afin de me connecter sans quitter l'application

  Contexte:
    Etant donné que j'ai demandé un code de connexion pour "test@example.com"

  Scénario: Code valide
    Quand je saisis le code "123456" et je valide
    Alors je suis redirigé vers le tableau de bord

  Scénario: Code refusé par le serveur
    Et que le serveur refusera le code saisi
    Quand je saisis le code "999999" et je valide
    Alors je vois le message d'erreur "Ce code est invalide ou a expiré."
    Et je reste sur la page de saisie du code
    Et le champ du code est vidé

  Scénario: Code incomplet
    Quand je saisis le code "123"
    Alors je ne peux pas valider le code

  Scénario: Les caractères autres que des chiffres sont ignorés
    Quand je saisis le code "123 456"
    Alors le champ du code contient "123456"
