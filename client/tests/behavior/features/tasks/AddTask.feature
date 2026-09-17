# language: fr
Fonctionnalité: Ajout d'une tâche à un événement
  En tant que participant d'un événement je veux ajouter une tâche (titre,
  catégorie, quantité) afin de compléter la liste de ce qu'il reste à faire

  Scénario: Ajout d'une tâche avec succès
    Quand j'arrive sur le détail de l'événement
    Et j'ajoute la tâche "Guirlandes" de catégorie "Logistique" et de quantité "2"
    Alors je vois la tâche "Guirlandes" de catégorie "Logistique"
    Et le formulaire d'ajout de tâche est réinitialisé

  Scénario: Ajout d'une tâche sans quantité
    Quand j'arrive sur le détail de l'événement
    Et j'ajoute la tâche "Réserver le DJ" de catégorie "Autre" sans quantité
    Alors je vois la tâche "Réserver le DJ" de catégorie "Autre"

  Scénario: Erreur lors de l'ajout d'une tâche
    Etant donné que l'ajout d'une tâche échoue
    Quand j'arrive sur le détail de l'événement
    Et j'ajoute la tâche "Guirlandes" de catégorie "Logistique" et de quantité "2"
    Alors je vois un message d'erreur pour l'ajout de tâche
