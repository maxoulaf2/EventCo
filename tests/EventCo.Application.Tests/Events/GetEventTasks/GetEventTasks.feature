# language: fr
Fonctionnalité: Consultation des tâches d'un événement
  En tant que participant d'un événement je veux consulter la liste de ses tâches
  afin de savoir ce qu'il reste à faire

  Scénario: Consultation des tâches d'un événement sans tâche
    Etant donné un événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice" dont je veux consulter les tâches
    Quand je consulte les tâches de cet événement
    Alors la consultation des tâches réussit
    Et l'événement consulté a 0 tâche

  Scénario: Consultation des tâches d'un événement avec plusieurs tâches
    Etant donné un événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice" dont je veux consulter les tâches
    Et une tâche "Bûche au chocolat" de catégorie "Courses" est ajoutée à cet événement
    Et une tâche "Réserver la salle" de catégorie "Logistique" est ajoutée à cet événement
    Quand je consulte les tâches de cet événement
    Alors la consultation des tâches réussit
    Et l'événement consulté a 2 tâches
    Et l'événement consulté a une tâche "Bûche au chocolat" de catégorie "Courses"

  Scénario: Consultation des tâches d'un événement inexistant
    Quand je consulte les tâches d'un événement inexistant
    Alors la consultation des tâches échoue avec une erreur d'événement introuvable

  Scénario: Consultation des tâches par un utilisateur qui n'est pas participant
    Etant donné un événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice" dont je veux consulter les tâches
    Et je change d'utilisateur courant
    Quand je consulte les tâches de cet événement
    Alors la consultation des tâches échoue avec une erreur d'autorisation

  Scénario: Consultation des tâches par un administrateur qui n'est pas participant
    Etant donné un événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice" dont je veux consulter les tâches
    Et une tâche "Bûche au chocolat" de catégorie "Courses" est ajoutée à cet événement
    Et je change d'utilisateur courant pour un administrateur
    Quand je consulte les tâches de cet événement
    Alors la consultation des tâches réussit
    Et l'événement consulté a 1 tâche
