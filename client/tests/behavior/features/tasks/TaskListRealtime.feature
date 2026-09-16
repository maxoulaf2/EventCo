# language: fr
Fonctionnalité: Mise à jour temps réel de la liste des tâches
  En tant que participant d'un événement je veux voir la liste des tâches se mettre à jour
  automatiquement quand un autre participant crée, termine ou supprime une tâche, sans recharger la page

  Scénario: Une tâche créée par un autre participant apparaît automatiquement
    Quand j'arrive sur le détail de l'événement
    Et un autre participant crée la tâche "Acheter des bougies" de catégorie "Courses"
    Alors je vois la tâche "Acheter des bougies" de catégorie "Courses"

  Scénario: Le statut d'une tâche se met à jour automatiquement
    Quand j'arrive sur le détail de l'événement
    Et un autre participant marque la tâche "Bûche au chocolat" comme faite
    Alors la tâche "Bûche au chocolat" apparaît comme faite

  Scénario: Une tâche supprimée par un autre participant disparaît automatiquement
    Quand j'arrive sur le détail de l'événement
    Et un autre participant supprime la tâche "Réserver la salle"
    Alors je ne vois plus la tâche "Réserver la salle"
