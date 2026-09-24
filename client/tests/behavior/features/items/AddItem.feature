# language: fr
Fonctionnalité: Ajout rapide d'un article à un événement
  En tant que participant d'un événement je veux ajouter rapidement un article
  par son titre afin de compléter la liste de ce qu'il reste à faire

  Scénario: Ajout d'un article avec succès
    Quand j'arrive sur le détail de l'événement
    Et j'ajoute l'article "Guirlandes"
    Alors je vois l'article "Guirlandes" dans la section "À prendre"
    Et le formulaire d'ajout d'article est réinitialisé

  Scénario: Erreur lors de l'ajout d'un article
    Etant donné que l'ajout d'un article échoue
    Quand j'arrive sur le détail de l'événement
    Et j'ajoute l'article "Guirlandes"
    Alors je vois un message d'erreur pour l'ajout d'article
