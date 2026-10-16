# language: fr
Fonctionnalité: Liste des articles d'un événement
  En tant que participant d'un événement je veux voir en tête ce qui reste à
  prendre, puis ce qui est déjà pris rangé par catégorie, afin de savoir en un
  coup d'œil ce qu'il reste à apporter et s'il ne manque rien

  Scénario: Les articles à prendre sont mis en valeur en tête de liste
    Quand j'arrive sur le détail de l'événement
    Alors je vois la section "À prendre" en premier, avec 1 article
    Et je vois l'article "Réserver la salle" à prendre

  Scénario: Les articles pris sont regroupés par catégorie, dans l'ordre fourni par l'API
    Quand j'arrive sur le détail de l'événement
    Alors je vois l'article "Bûche au chocolat" dans la catégorie "Desserts"
    Et je vois l'article "Vin rouge" dans la catégorie "Boissons"
    Et les catégories sont affichées dans l'ordre "Desserts", "Boissons"
    Et la catégorie "Boissons" est affichée avec son libellé

  Scénario: Les catégories sans article pris ne sont pas affichées
    Quand j'arrive sur le détail de l'événement
    Alors je ne vois pas la catégorie "Apéro"
    Et je ne vois pas la catégorie "Autre"

  Scénario: Sans article à prendre, la section "À prendre" n'est pas affichée
    Etant donné que l'article "Réserver la salle" est assigné à un participant
    Quand j'arrive sur le détail de l'événement
    Alors je ne vois pas la section "À prendre"

  Scénario: Événement sans article
    Etant donné que cet événement n'a aucun article
    Quand j'arrive sur le détail de l'événement
    Alors je vois un message indiquant qu'il n'y a aucun article pour le moment
