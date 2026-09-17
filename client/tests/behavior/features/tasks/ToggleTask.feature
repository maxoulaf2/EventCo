# language: fr
Fonctionnalité: Basculer le statut d'une tâche
  En tant que participant d'un événement je veux pouvoir cocher/décocher une tâche
  en un geste, et ne pouvoir cocher que mes propres tâches si je ne suis pas
  créateur ou co-organisateur

  Scénario: Le créateur coche une tâche à faire
    Quand j'arrive sur le détail de l'événement
    Et je coche la tâche "Bûche au chocolat"
    Alors la tâche "Bûche au chocolat" est cochée

  Scénario: Le créateur décoche une tâche faite
    Quand j'arrive sur le détail de l'événement
    Et je décoche la tâche "Réserver la salle"
    Alors la tâche "Réserver la salle" n'est pas cochée

  Scénario: Un participant coche sa propre tâche assignée
    Etant donné que je suis un simple participant assigné à la tâche "Bûche au chocolat"
    Quand j'arrive sur le détail de l'événement
    Et je coche la tâche "Bûche au chocolat"
    Alors la tâche "Bûche au chocolat" est cochée

  Scénario: Un participant ne peut pas cocher une tâche qui n'est pas la sienne
    Etant donné que je suis un simple participant non assigné à la tâche "Réserver la salle"
    Quand j'arrive sur le détail de l'événement
    Alors la case de la tâche "Réserver la salle" est désactivée

  Scénario: Erreur lors du basculement d'une tâche
    Etant donné que le basculement de la tâche "Bûche au chocolat" échoue
    Quand j'arrive sur le détail de l'événement
    Et je coche la tâche "Bûche au chocolat"
    Alors la tâche "Bûche au chocolat" n'est pas cochée
