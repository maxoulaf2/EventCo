# language: fr
Fonctionnalité: Demande de lien de connexion (magic link)
  En tant qu'utilisateur je veux demander un lien de connexion depuis le formulaire
  afin de recevoir un email me permettant de me connecter sans mot de passe

  Contexte:
    Etant donné que je suis sur la page de connexion

  Scénario: Email valide
    Quand je saisis l'email "test@example.com" et je valide le formulaire
    Alors je suis redirigé vers la page de confirmation
    Et je vois l'email "test@example.com" affiché

  Scénario: Le serveur refuse la demande
    Et que le serveur refusera la prochaine demande de lien
    Quand je saisis l'email "test@example.com" et je valide le formulaire
    Alors je vois un message d'erreur sur le formulaire
    Et je reste sur la page de connexion
