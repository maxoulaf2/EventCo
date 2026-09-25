# language: fr
Fonctionnalité: Mise à jour temps réel de la liste des articles
  En tant que participant d'un événement je veux voir la liste des articles se mettre à jour
  automatiquement quand un autre participant crée, assigne ou supprime un article, sans recharger la page

  Scénario: Un article créé par un autre participant apparaît automatiquement
    Quand j'arrive sur le détail de l'événement
    Et un autre participant crée l'article "Acheter des bougies"
    Alors je vois l'article "Acheter des bougies"

  Scénario: L'assignation d'un article se met à jour automatiquement
    Quand j'arrive sur le détail de l'événement
    Et un autre participant assigne l'article "Bûche au chocolat"
    Alors je vois l'article "Bûche au chocolat" dans la section "Assignés"

  Scénario: Un article supprimé par un autre participant disparaît automatiquement
    Quand j'arrive sur le détail de l'événement
    Et un autre participant supprime l'article "Réserver la salle"
    Alors je ne vois plus l'article "Réserver la salle"

  Scénario: La version de l'API est vérifiée à la reconnexion temps réel
    Quand j'arrive sur le détail de l'événement
    Et la connexion temps réel est rétablie après une coupure
    Alors la version de l'API est vérifiée
