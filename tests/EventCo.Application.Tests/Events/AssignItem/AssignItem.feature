# language: fr
Fonctionnalité: Assignation d'un article à un participant
  En tant que participant d'un événement je veux assigner un article à moi-même ou à un autre participant
  afin de savoir qui s'occupe de quoi

  Scénario: Auto-assignation par un participant simple
    Etant donné un événement "Repas de Noël" avec un article "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et "ami@example.com" rejoint cet événement en tant que participant simple
    Et ce participant devient l'utilisateur courant
    Quand j'assigne cet article à moi-même
    Alors l'assignation réussit
    Et l'article est assigné à ce participant
    Et une notification temps réel d'assignation d'article est diffusée

  Scénario: Assignation par le créateur à un autre participant
    Etant donné un événement "Repas de Noël" avec un article "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et "ami@example.com" rejoint cet événement en tant que participant simple
    Quand j'assigne cet article à ce participant
    Alors l'assignation réussit
    Et l'article est assigné à ce participant
    Et une notification temps réel d'assignation d'article est diffusée

  Scénario: Assignation par un participant simple à un autre participant
    Etant donné un événement "Repas de Noël" avec un article "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et "ami@example.com" rejoint cet événement en tant que participant simple
    Et "autre@example.com" rejoint cet événement en tant que participant simple
    Et ce participant devient l'utilisateur courant
    Quand j'assigne cet article à l'autre participant
    Alors l'assignation échoue avec une erreur d'auto-assignation

  Scénario: Assignation à un utilisateur qui n'est pas participant
    Etant donné un événement "Repas de Noël" avec un article "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'assigne cet article à un utilisateur qui n'est pas participant
    Alors l'assignation échoue avec une erreur d'assigné invalide

  Scénario: Assignation par un utilisateur qui ne participe pas à l'événement
    Etant donné un événement "Repas de Noël" avec un article "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand j'assigne cet article à moi-même
    Alors l'assignation échoue avec une erreur d'autorisation

  Scénario: Assignation sur un événement inexistant
    Quand j'assigne un article à un événement inexistant
    Alors l'assignation échoue avec une erreur d'événement introuvable

  Scénario: Assignation d'un article inexistant
    Etant donné un événement "Repas de Noël" avec un article "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'assigne un article inexistant à moi-même
    Alors l'assignation échoue avec une erreur d'article introuvable

  Scénario: Réattribution refusée d'un article apporté par un participant
    Etant donné un événement "Repas de Noël" avec un article "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et "ami@example.com" rejoint cet événement en tant que participant simple
    Et ce participant apporte l'article "Chips"
    Quand j'assigne cet article à moi-même
    Alors l'assignation échoue avec une erreur d'article apporté
