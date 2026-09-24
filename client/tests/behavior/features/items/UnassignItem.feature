# language: fr
Fonctionnalité: Désassignation d'un article
  En tant que participant d'un événement je veux pouvoir me désassigner d'un
  article que j'ai pris, et le créateur/co-organisateur doit pouvoir désassigner
  l'article de n'importe quel participant

  Scénario: Le créateur désassigne l'article d'un autre participant
    Etant donné que l'article "Bûche au chocolat" est assigné à un participant
    Quand j'arrive sur le détail de l'événement
    Et je désassigne l'article "Bûche au chocolat"
    Alors je vois l'article "Bûche au chocolat" dans la section "À prendre"

  Scénario: Un participant se désassigne de son propre article
    Etant donné que je suis un simple participant assigné à l'article "Bûche au chocolat"
    Quand j'arrive sur le détail de l'événement
    Et je désassigne l'article "Bûche au chocolat"
    Alors je vois l'article "Bûche au chocolat" dans la section "À prendre"

  Scénario: Un participant ne peut pas désassigner l'article d'un autre
    Etant donné que je suis un simple participant et que l'article "Réserver la salle" est assigné à quelqu'un d'autre
    Quand j'arrive sur le détail de l'événement
    Alors je ne vois pas de bouton de désassignation pour l'article "Réserver la salle"

  Scénario: Erreur lors de la désassignation d'un article
    Etant donné que l'article "Bûche au chocolat" est assigné à un participant
    Et la désassignation de l'article "Bûche au chocolat" échoue
    Quand j'arrive sur le détail de l'événement
    Et je désassigne l'article "Bûche au chocolat"
    Alors je vois l'article "Bûche au chocolat" dans la section "Assignés"
