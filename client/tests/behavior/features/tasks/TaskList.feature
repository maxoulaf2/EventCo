# language: fr
Fonctionnalité: Liste des tâches d'un événement
  En tant que participant d'un événement je veux voir ses tâches réparties par
  statut (à prendre / assignées / faites) afin de savoir en un coup d'œil ce
  qu'il reste à faire

  Scénario: Affichage des tâches à prendre
    Quand j'arrive sur le détail de l'événement
    Alors je vois la tâche "Bûche au chocolat" sur l'onglet "À prendre"
    Et je vois la tâche "Réserver la salle" sur l'onglet "À prendre"

  Scénario: Une tâche assignée apparaît sur l'onglet "Assignées"
    Etant donné que la tâche "Bûche au chocolat" est assignée à un participant
    Quand j'arrive sur le détail de l'événement
    Et je vais sur l'onglet "Assignées"
    Alors je vois la tâche "Bûche au chocolat" sur l'onglet "Assignées"
    Et je ne vois pas la tâche "Réserver la salle"

  Scénario: Une tâche faite apparaît sur l'onglet "Faites"
    Etant donné que la tâche "Réserver la salle" est faite
    Quand j'arrive sur le détail de l'événement
    Et je vais sur l'onglet "Faites"
    Alors je vois la tâche "Réserver la salle" sur l'onglet "Faites"
    Et je ne vois pas la tâche "Bûche au chocolat"

  Scénario: Aucune tâche sur un onglet
    Quand j'arrive sur le détail de l'événement
    Et je vais sur l'onglet "Faites"
    Alors je vois un message indiquant qu'il n'y a aucune tâche sur cet onglet

  Scénario: Événement sans tâche
    Etant donné que cet événement n'a aucune tâche
    Quand j'arrive sur le détail de l'événement
    Alors je vois un message indiquant qu'il n'y a aucune tâche pour le moment
