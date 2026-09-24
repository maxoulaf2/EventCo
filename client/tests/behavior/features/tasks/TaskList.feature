# language: fr
Fonctionnalité: Liste des tâches d'un événement
  En tant que participant d'un événement je veux voir toutes ses tâches d'un
  coup, réparties en deux sections (à prendre / assignées) afin de savoir en
  un coup d'œil ce qu'il reste à faire

  Scénario: Affichage des tâches à prendre
    Quand j'arrive sur le détail de l'événement
    Alors je vois la tâche "Bûche au chocolat" dans la section "À prendre"
    Et je vois la tâche "Réserver la salle" dans la section "À prendre"

  Scénario: Tâches à prendre et assignées affichées ensemble
    Etant donné que la tâche "Bûche au chocolat" est assignée à un participant
    Quand j'arrive sur le détail de l'événement
    Alors je vois la tâche "Bûche au chocolat" dans la section "Assignées"
    Et je vois la tâche "Réserver la salle" dans la section "À prendre"

  Scénario: Aucune tâche dans une section
    Etant donné que cet événement n'a aucune tâche assignée
    Quand j'arrive sur le détail de l'événement
    Alors je vois un message indiquant qu'il n'y a aucune tâche dans la section "Assignées"

  Scénario: Événement sans tâche
    Etant donné que cet événement n'a aucune tâche
    Quand j'arrive sur le détail de l'événement
    Alors je vois un message indiquant qu'il n'y a aucune tâche pour le moment
