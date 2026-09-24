# language: fr
Fonctionnalité: Rejoindre un événement via un lien d'invitation
  En tant qu'utilisateur je veux cliquer sur un lien d'invitation partagé
  afin de rejoindre l'événement, que je sois déjà connecté ou non

  Scénario: Un utilisateur déjà connecté rejoint automatiquement l'événement
    Etant donné que je suis connecté
    Quand j'ouvre le lien d'invitation "invite-token-1"
    Alors je suis redirigé vers la page de l'événement

  Scénario: Un utilisateur non connecté voit un aperçu, se connecte avec le code reçu et rejoint l'événement
    Etant donné que je ne suis pas connecté
    Quand j'ouvre le lien d'invitation "invite-token-1"
    Alors je vois l'aperçu de l'événement "Repas de Noël"
    Quand je saisis l'email "invite@example.com" et je valide le formulaire de connexion
    Alors je suis redirigé vers la page de confirmation
    Quand je saisis le code reçu et je valide
    Alors je suis redirigé vers la page de l'événement rejoint

  Scénario: Lien d'invitation invalide pour un utilisateur non connecté
    Etant donné que je ne suis pas connecté
    Et que le lien d'invitation n'est plus valide
    Quand j'ouvre le lien d'invitation "token-invalide"
    Alors je vois un message indiquant que le lien n'est plus valide
