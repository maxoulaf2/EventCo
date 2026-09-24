# language: fr
Fonctionnalité: Désassignation d'une tâche
  En tant que participant d'un événement je veux pouvoir me désassigner d'une
  tâche que j'ai prise, et le créateur/co-organisateur doit pouvoir désassigner
  la tâche de n'importe quel participant

  Scénario: Le créateur désassigne la tâche d'un autre participant
    Etant donné que la tâche "Bûche au chocolat" est assignée à un participant
    Quand j'arrive sur le détail de l'événement
    Et je désassigne la tâche "Bûche au chocolat"
    Alors je vois la tâche "Bûche au chocolat" dans la section "À prendre"

  Scénario: Un participant se désassigne de sa propre tâche
    Etant donné que je suis un simple participant assigné à la tâche "Bûche au chocolat"
    Quand j'arrive sur le détail de l'événement
    Et je désassigne la tâche "Bûche au chocolat"
    Alors je vois la tâche "Bûche au chocolat" dans la section "À prendre"

  Scénario: Un participant ne peut pas désassigner la tâche d'un autre
    Etant donné que je suis un simple participant et que la tâche "Réserver la salle" est assignée à quelqu'un d'autre
    Quand j'arrive sur le détail de l'événement
    Alors je ne vois pas de bouton de désassignation pour la tâche "Réserver la salle"

  Scénario: Erreur lors de la désassignation d'une tâche
    Etant donné que la tâche "Bûche au chocolat" est assignée à un participant
    Et la désassignation de la tâche "Bûche au chocolat" échoue
    Quand j'arrive sur le détail de l'événement
    Et je désassigne la tâche "Bûche au chocolat"
    Alors je vois la tâche "Bûche au chocolat" dans la section "Assignées"
