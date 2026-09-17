# language: fr
Fonctionnalité: Ajout rapide d'une tâche à un événement
  En tant que participant d'un événement je veux ajouter rapidement une tâche
  par son titre afin de compléter la liste de ce qu'il reste à faire

  Scénario: Ajout d'une tâche avec succès
    Quand j'arrive sur le détail de l'événement
    Et j'ajoute la tâche "Guirlandes"
    Alors je vois la tâche "Guirlandes" sur l'onglet "À prendre"
    Et le formulaire d'ajout de tâche est réinitialisé

  Scénario: Erreur lors de l'ajout d'une tâche
    Etant donné que l'ajout d'une tâche échoue
    Quand j'arrive sur le détail de l'événement
    Et j'ajoute la tâche "Guirlandes"
    Alors je vois un message d'erreur pour l'ajout de tâche
