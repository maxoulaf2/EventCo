# language: fr
Fonctionnalité: Assignation d'une tâche à un participant
  En tant que participant d'un événement je veux assigner une tâche à moi-même ou à un autre participant
  afin de savoir qui s'occupe de quoi

  Scénario: Auto-assignation par un participant simple
    Etant donné un événement "Repas de Noël" avec une tâche "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et "ami@example.com" rejoint cet événement en tant que participant simple
    Et ce participant devient l'utilisateur courant
    Quand j'assigne cette tâche à moi-même
    Alors l'assignation réussit
    Et la tâche est assignée à ce participant

  Scénario: Assignation par le créateur à un autre participant
    Etant donné un événement "Repas de Noël" avec une tâche "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et "ami@example.com" rejoint cet événement en tant que participant simple
    Quand j'assigne cette tâche à ce participant
    Alors l'assignation réussit
    Et la tâche est assignée à ce participant

  Scénario: Assignation par un participant simple à un autre participant
    Etant donné un événement "Repas de Noël" avec une tâche "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et "ami@example.com" rejoint cet événement en tant que participant simple
    Et "autre@example.com" rejoint cet événement en tant que participant simple
    Et ce participant devient l'utilisateur courant
    Quand j'assigne cette tâche à l'autre participant
    Alors l'assignation échoue avec une erreur d'auto-assignation

  Scénario: Assignation à un utilisateur qui n'est pas participant
    Etant donné un événement "Repas de Noël" avec une tâche "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'assigne cette tâche à un utilisateur qui n'est pas participant
    Alors l'assignation échoue avec une erreur d'assigné invalide

  Scénario: Assignation par un utilisateur qui ne participe pas à l'événement
    Etant donné un événement "Repas de Noël" avec une tâche "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand j'assigne cette tâche à moi-même
    Alors l'assignation échoue avec une erreur d'autorisation

  Scénario: Assignation sur un événement inexistant
    Quand j'assigne une tâche à un événement inexistant
    Alors l'assignation échoue avec une erreur d'événement introuvable
