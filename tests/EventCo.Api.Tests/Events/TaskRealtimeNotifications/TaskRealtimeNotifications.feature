# language: fr
Fonctionnalité: Diffusion temps réel des événements de tâches
  En tant que participant connecté au hub temps réel d'un événement je veux recevoir les mises à jour de tâches en direct
  afin de suivre les préparatifs sans recharger la page

  Scénario: La création d'une tâche est diffusée au groupe temps réel
    Etant donné une session ouverte via l'API pour "realtime-create-task-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et je me connecte au hub temps réel et que je rejoins cet événement
    Et j'écoute les notifications temps réel de tâches
    Quand j'ajoute la tâche "Bûche au chocolat" de quantité "1" à cet événement via l'API
    Alors une notification temps réel de création de tâche est reçue pour cette tâche

  Scénario: L'assignation d'une tâche est diffusée au groupe temps réel
    Etant donné une session ouverte via l'API pour "realtime-assign-task-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et "realtime-assign-task-participant-api-test@example.com" invité à cet événement via l'API
    Et je me connecte au hub temps réel et que je rejoins cet événement
    Et j'écoute les notifications temps réel de tâches
    Quand j'assigne cette tâche à ce participant via l'API
    Alors une notification temps réel d'assignation de tâche est reçue pour cette tâche

  Scénario: La suppression d'une tâche est diffusée au groupe temps réel
    Etant donné une session ouverte via l'API pour "realtime-delete-task-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une tâche "Bûche au chocolat" ajoutée à cet événement via l'API
    Et je me connecte au hub temps réel et que je rejoins cet événement
    Et j'écoute les notifications temps réel de tâches
    Quand je supprime cette tâche via l'API
    Alors une notification temps réel de suppression de tâche est reçue pour cette tâche
