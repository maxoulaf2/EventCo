# language: fr
Fonctionnalité: Mise à jour temps réel de la liste des tâches
  En tant que participant d'un événement je veux voir la liste des tâches se mettre à jour
  automatiquement quand un autre participant crée, assigne ou supprime une tâche, sans recharger la page

  Scénario: Une tâche créée par un autre participant apparaît automatiquement
    Quand j'arrive sur le détail de l'événement
    Et un autre participant crée la tâche "Acheter des bougies" de catégorie "Courses"
    Alors je vois la tâche "Acheter des bougies" de catégorie "Courses"

  Scénario: L'assignation d'une tâche se met à jour automatiquement
    Quand j'arrive sur le détail de l'événement
    Et un autre participant assigne la tâche "Bûche au chocolat"
    Et je vais sur l'onglet "Assignées"
    Alors je vois la tâche "Bûche au chocolat" sur l'onglet "Assignées"

  Scénario: Une tâche supprimée par un autre participant disparaît automatiquement
    Quand j'arrive sur le détail de l'événement
    Et un autre participant supprime la tâche "Réserver la salle"
    Alors je ne vois plus la tâche "Réserver la salle"
