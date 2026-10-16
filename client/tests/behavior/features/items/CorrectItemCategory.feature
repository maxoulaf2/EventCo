# language: fr
Fonctionnalité: Correction de la catégorie d'un article
  En tant que participant d'un événement je veux pouvoir corriger la catégorie
  déduite automatiquement de ce que j'apporte, et en tant qu'organisateur celle
  de n'importe quel article, afin que la liste reste bien rangée

  Scénario: Un organisateur corrige la catégorie d'un article
    Quand j'arrive sur le détail de l'événement
    Et je range l'article "Réserver la salle" dans la catégorie "Matériel et déco"
    Alors la catégorie "Equipment" est envoyée pour l'article "Réserver la salle"
    Et je vois l'article "Réserver la salle" dans la catégorie "Matériel et déco"

  Scénario: Un participant simple corrige la catégorie de ce qu'il apporte
    Etant donné que je suis un simple participant qui apporte l'article "Le truc de mamie"
    Quand j'arrive sur le détail de l'événement
    Alors je peux corriger la catégorie de l'article "Le truc de mamie"

  Scénario: Un participant simple ne peut pas corriger la catégorie d'un article à prendre
    Etant donné que je suis un simple participant
    Quand j'arrive sur le détail de l'événement
    Alors je ne peux pas corriger la catégorie de l'article "Réserver la salle"

  Scénario: Erreur lors de la correction de la catégorie
    Etant donné que la correction de catégorie échoue
    Quand j'arrive sur le détail de l'événement
    Et je range l'article "Réserver la salle" dans la catégorie "Matériel et déco"
    Alors je vois l'article "Réserver la salle" dans la catégorie "Autre"
