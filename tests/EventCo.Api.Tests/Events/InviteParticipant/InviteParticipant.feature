# language: fr
Fonctionnalité: Invitation d'un participant via l'API
  En tant qu'utilisateur connecté je veux inviter quelqu'un par email à un événement via l'API
  afin qu'il puisse y participer

  Scénario: Invitation réussie
    Etant donné une session ouverte via l'API pour "invite-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand j'invite "invite-guest-api-test@example.com" à cet événement via l'API
    Alors la réponse d'invitation a le statut 201

  Scénario: Invitation sur un événement inexistant
    Etant donné une session ouverte via l'API pour "invite-missing-event-api-test@example.com"
    Quand j'invite "invite-guest-api-test@example.com" à un événement inexistant via l'API
    Alors la réponse d'invitation a le statut 404

  Scénario: Invitation sans cookie de session
    Quand j'invite "invite-guest-api-test@example.com" à un événement inexistant via l'API sans cookie de session
    Alors la réponse d'invitation a le statut 401

  Scénario: Invitation avec un email invalide
    Etant donné une session ouverte via l'API pour "invite-invalid-email-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand j'invite "pas-un-email" à cet événement via l'API
    Alors la réponse d'invitation a le statut 400
