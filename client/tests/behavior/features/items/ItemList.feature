# language: fr
Fonctionnalité: Liste des articles d'un événement
  En tant que participant d'un événement je veux voir tous ses articles d'un
  coup, répartis en deux sections (à prendre / assignés) afin de savoir en
  un coup d'œil ce qu'il reste à apporter

  Scénario: Affichage des articles à prendre
    Quand j'arrive sur le détail de l'événement
    Alors je vois l'article "Bûche au chocolat" dans la section "À prendre"
    Et je vois l'article "Réserver la salle" dans la section "À prendre"

  Scénario: Articles à prendre et assignés affichés ensemble
    Etant donné que l'article "Bûche au chocolat" est assigné à un participant
    Quand j'arrive sur le détail de l'événement
    Alors je vois l'article "Bûche au chocolat" dans la section "Assignés"
    Et je vois l'article "Réserver la salle" dans la section "À prendre"

  Scénario: Aucun article dans une section
    Etant donné que cet événement n'a aucun article assigné
    Quand j'arrive sur le détail de l'événement
    Alors je vois un message indiquant qu'il n'y a aucun article dans la section "Assignés"

  Scénario: Événement sans article
    Etant donné que cet événement n'a aucun article
    Quand j'arrive sur le détail de l'événement
    Alors je vois un message indiquant qu'il n'y a aucun article pour le moment
