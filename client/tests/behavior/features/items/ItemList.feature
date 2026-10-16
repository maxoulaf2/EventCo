# language: fr
Fonctionnalité: Liste des articles d'un événement
  En tant que participant d'un événement je veux voir ses articles rangés par
  catégorie afin de savoir en un coup d'œil s'il ne manque rien et ce qu'il
  reste à prendre

  Scénario: Articles regroupés par catégorie, dans l'ordre fourni par l'API
    Quand j'arrive sur le détail de l'événement
    Alors je vois l'article "Bûche au chocolat" dans la catégorie "Desserts"
    Et je vois l'article "Vin rouge" dans la catégorie "Boissons"
    Et je vois l'article "Réserver la salle" dans la catégorie "Autre"
    Et les catégories sont affichées dans l'ordre "Desserts", "Boissons", "Autre"
    Et la catégorie "Boissons" est affichée avec son libellé

  Scénario: Les catégories sans article ne sont pas affichées
    Quand j'arrive sur le détail de l'événement
    Alors je ne vois pas la catégorie "Apéro"

  Scénario: Nombre d'articles restant à prendre
    Etant donné que l'article "Bûche au chocolat" est assigné à un participant
    Quand j'arrive sur le détail de l'événement
    Alors je vois qu'il reste 2 articles à prendre

  Scénario: Événement sans article
    Etant donné que cet événement n'a aucun article
    Quand j'arrive sur le détail de l'événement
    Alors je vois un message indiquant qu'il n'y a aucun article pour le moment
