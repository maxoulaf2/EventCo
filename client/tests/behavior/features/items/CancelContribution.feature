# language: fr
Fonctionnalité: Annulation d'un article apporté
  En tant que participant d'un événement je veux pouvoir annuler un article que j'apporte (ajouté par moi,
  ou à prendre que j'ai pris), sans pouvoir le remettre à prendre, et personne d'autre ne doit pouvoir
  l'annuler ou le remettre à prendre

  Scénario: La personne qui apporte un article l'annule
    Etant donné que je suis un simple participant qui apporte l'article "Chips"
    Quand j'arrive sur le détail de l'événement
    Alors je ne vois pas de bouton de désassignation pour l'article "Chips"
    Quand j'annule l'article "Chips"
    Alors je ne vois plus l'article "Chips"

  Scénario: Un organisateur ne peut ni désassigner ni annuler l'article apporté par un participant
    Etant donné que l'article "Chips" est apporté par un participant
    Quand j'arrive sur le détail de l'événement
    Alors je ne vois ni bouton de désassignation ni bouton d'annulation pour l'article "Chips"

  Scénario: Erreur lors de l'annulation d'un article apporté
    Etant donné que je suis un simple participant qui apporte l'article "Chips"
    Et l'annulation de l'article "Chips" échoue
    Quand j'arrive sur le détail de l'événement
    Et j'annule l'article "Chips"
    Alors je vois toujours l'article "Chips" assigné

  Scénario: Un participant annule l'article à prendre qu'il a pris
    Etant donné que je suis un simple participant qui a pris l'article à prendre "Chips"
    Quand j'arrive sur le détail de l'événement
    Alors je ne vois pas de bouton de désassignation pour l'article "Chips"
    Quand j'annule l'article "Chips"
    Alors je ne vois plus l'article "Chips"

  Scénario: Un organisateur ne peut ni remettre à prendre ni annuler un article à prendre pris par un participant
    Etant donné que l'article à prendre "Chips" est pris par un participant
    Quand j'arrive sur le détail de l'événement
    Alors je ne vois ni bouton de désassignation ni bouton d'annulation pour l'article "Chips"
