# language: fr
Fonctionnalité: Connexion au groupe temps réel d'un événement (SignalR)
  En tant que participant à un événement je veux rejoindre son groupe temps réel
  afin de recevoir les mises à jour de tâches en direct

  Scénario: Un participant rejoint le groupe temps réel de l'événement
    Etant donné une session ouverte via l'API pour "join-hub-participant-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand je me connecte au hub temps réel et que je rejoins cet événement
    Alors la connexion au groupe temps réel réussit

  Scénario: Un utilisateur qui n'est pas participant ne peut pas rejoindre le groupe temps réel
    Etant donné une session ouverte via l'API pour "join-hub-creator-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une session ouverte via l'API pour "join-hub-other-user-api-test@example.com"
    Quand je me connecte au hub temps réel et que je rejoins cet événement
    Alors la connexion au groupe temps réel échoue avec une erreur d'autorisation

  Scénario: Impossible de se connecter au hub temps réel sans cookie de session
    Quand je me connecte au hub temps réel sans cookie de session
    Alors la connexion au hub temps réel est refusée
