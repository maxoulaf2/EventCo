# language: fr
Fonctionnalité: Rejoindre un événement via un lien d'invitation via l'API
  En tant qu'utilisateur connecté je veux rejoindre un événement via son lien d'invitation
  afin de devenir participant sans invitation par email

  Scénario: Rejoindre avec un lien valide
    Etant donné une session ouverte via l'API pour "join-link-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une session ouverte via l'API pour "join-link-guest-api-test@example.com"
    Quand je rejoins cet événement via son lien d'invitation via l'API
    Alors la réponse de participation via lien a le statut 200

  Scénario: Rejoindre avec un lien d'invitation inexistant
    Etant donné une session ouverte via l'API pour "join-link-missing-token-api-test@example.com"
    Quand je rejoins un événement via un lien d'invitation inexistant via l'API
    Alors la réponse de participation via lien a le statut 404

  Scénario: Rejoindre sans cookie de session
    Etant donné une session ouverte via l'API pour "join-link-anon-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand je rejoins cet événement via son lien d'invitation via l'API sans cookie de session
    Alors la réponse de participation via lien a le statut 401
