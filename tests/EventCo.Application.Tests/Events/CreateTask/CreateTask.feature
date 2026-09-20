# language: fr
Fonctionnalité: Création d'une tâche sur un événement
  En tant que participant d'un événement je veux ajouter une tâche
  afin de répartir ce qu'il reste à faire

  Scénario: Création d'une tâche par le créateur
    Etant donné un événement ouvert à l'ajout de tâches "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'ajoute la tâche "Bûche au chocolat" de catégorie "Courses" et de quantité "1" à cet événement
    Alors la création de la tâche réussit
    Et la tâche créée a pour titre "Bûche au chocolat"
    Et la tâche créée a pour catégorie "Courses"
    Et une notification temps réel de création de tâche est diffusée

  Scénario: Création d'une tâche par un participant simple (non organisateur)
    Etant donné un événement ouvert à l'ajout de tâches "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et un participant "ami@example.com" a rejoint cet événement
    Et j'agis désormais en tant que ce participant
    Quand j'ajoute la tâche "Bûche au chocolat" de catégorie "Courses" et de quantité "1" à cet événement
    Alors la création de la tâche réussit

  Scénario: Création d'une tâche avec un titre vide
    Etant donné un événement ouvert à l'ajout de tâches "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'ajoute la tâche " " de catégorie "Courses" et de quantité "1" à cet événement
    Alors la création de la tâche échoue avec une erreur de validation

  Scénario: Création d'une tâche avec une catégorie invalide
    Etant donné un événement ouvert à l'ajout de tâches "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'ajoute la tâche "Bûche au chocolat" de catégorie "Desserts" et de quantité "1" à cet événement
    Alors la création de la tâche échoue avec une erreur de validation

  Scénario: Création d'une tâche sur un événement inexistant
    Quand j'ajoute la tâche "Bûche au chocolat" de catégorie "Courses" et de quantité "1" à un événement inexistant
    Alors la création de la tâche échoue avec une erreur d'événement introuvable

  Scénario: Création d'une tâche par un utilisateur qui ne participe pas à l'événement
    Etant donné un événement ouvert à l'ajout de tâches "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand j'ajoute la tâche "Bûche au chocolat" de catégorie "Courses" et de quantité "1" à cet événement
    Alors la création de la tâche échoue avec une erreur d'autorisation
