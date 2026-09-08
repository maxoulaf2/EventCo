# language: fr
Fonctionnalité: Rétrogradation d'un co-organisateur en participant
  En tant que créateur d'un événement je veux rétrograder un co-organisateur en simple participant
  afin de reprendre la main sur la gestion de l'événement

  Scénario: Rétrogradation par le créateur
    Etant donné un événement "Repas de Noël" avec un co-organisateur "ami@example.com", prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je rétrograde ce co-organisateur en participant
    Alors la rétrogradation réussit
    Et le co-organisateur rétrogradé a le rôle "Participant"

  Scénario: Rétrogradation par un utilisateur qui n'est pas le créateur
    Etant donné un événement "Repas de Noël" avec un co-organisateur "ami@example.com", prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand je rétrograde ce co-organisateur en participant
    Alors la rétrogradation échoue avec une erreur d'autorisation

  Scénario: Rétrogradation du créateur lui-même
    Etant donné un événement "Repas de Noël" avec un co-organisateur "ami@example.com", prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je rétrograde le créateur de l'événement
    Alors la rétrogradation échoue car le créateur ne peut pas être rétrogradé

  Scénario: Rétrogradation sur un événement inexistant
    Quand je rétrograde un co-organisateur sur un événement inexistant
    Alors la rétrogradation échoue avec une erreur d'événement introuvable
