# language: fr
Fonctionnalité: Liste des tâches d'un événement
  En tant que participant d'un événement je veux voir la liste de ses tâches
  et pouvoir la filtrer par catégorie afin de savoir ce qu'il reste à faire

  Scénario: Affichage des tâches de toutes les catégories
    Quand j'arrive sur le détail de l'événement
    Alors je vois la tâche "Bûche au chocolat" de catégorie "Courses"
    Et je vois la tâche "Réserver la salle" de catégorie "Logistique"

  Scénario: Filtrage des tâches par catégorie
    Quand j'arrive sur le détail de l'événement
    Et je filtre les tâches par catégorie "Courses"
    Alors je vois la tâche "Bûche au chocolat" de catégorie "Courses"
    Et je ne vois pas la tâche "Réserver la salle"

  Scénario: Retour au filtre "Toutes" après un filtrage
    Quand j'arrive sur le détail de l'événement
    Et je filtre les tâches par catégorie "Courses"
    Et je filtre les tâches par catégorie "Toutes"
    Alors je vois la tâche "Bûche au chocolat" de catégorie "Courses"
    Et je vois la tâche "Réserver la salle" de catégorie "Logistique"

  Scénario: Aucune tâche dans la catégorie sélectionnée
    Quand j'arrive sur le détail de l'événement
    Et je filtre les tâches par catégorie "Autre"
    Alors je vois un message indiquant qu'il n'y a aucune tâche dans cette catégorie

  Scénario: Événement sans tâche
    Etant donné que cet événement n'a aucune tâche
    Quand j'arrive sur le détail de l'événement
    Alors je vois un message indiquant qu'il n'y a aucune tâche pour le moment
