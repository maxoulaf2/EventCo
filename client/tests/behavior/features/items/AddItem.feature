# language: fr
Fonctionnalité: Ajout rapide d'un article à un événement
  En tant que participant d'un événement je veux ajouter rapidement ce que j'apporte,
  et en tant qu'organisateur je veux ajouter ce qu'il faudrait apporter (que je peux ensuite m'assigner)

  Scénario: Un organisateur ajoute un article à prendre
    Quand j'arrive sur le détail de l'événement
    Et j'ajoute l'article "Guirlandes"
    Alors l'article est envoyé comme "à prendre"
    Et je vois l'article "Guirlandes" dans la section "À prendre"
    Et le formulaire d'ajout d'article est réinitialisé

  Scénario: Un participant simple ajoute ce qu'il apporte
    Etant donné que je suis un simple participant
    Quand j'arrive sur le détail de l'événement
    Et j'ajoute l'article "Guirlandes"
    Alors l'article est envoyé comme "apporté"
    Et je vois l'article "Guirlandes" dans la section "Assignés"

  Scénario: Erreur lors de l'ajout d'un article
    Etant donné que l'ajout d'un article échoue
    Quand j'arrive sur le détail de l'événement
    Et j'ajoute l'article "Guirlandes"
    Alors je vois un message d'erreur pour l'ajout d'article
