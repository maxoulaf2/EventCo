# language: fr
Fonctionnalité: Promotion d'un participant en co-organisateur
  En tant que créateur d'un événement je veux promouvoir un participant en co-organisateur
  afin de partager la gestion de l'événement

  Scénario: Promotion par le créateur
    Etant donné un événement "Repas de Noël" avec un participant invité "ami@example.com", prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je promeus ce participant en co-organisateur
    Alors la promotion réussit
    Et le participant a le rôle "Organizer"

  Scénario: Promotion par un utilisateur qui n'est pas le créateur
    Etant donné un événement "Repas de Noël" avec un participant invité "ami@example.com", prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand je promeus ce participant en co-organisateur
    Alors la promotion échoue avec une erreur d'autorisation

  Scénario: Promotion d'un utilisateur qui ne participe pas à l'événement
    Etant donné un événement "Repas de Noël" avec un participant invité "ami@example.com", prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je promeus un utilisateur qui ne participe pas à l'événement
    Alors la promotion échoue avec une erreur de participant introuvable

  Scénario: Promotion sur un événement inexistant
    Quand je promeus un participant sur un événement inexistant
    Alors la promotion échoue avec une erreur d'événement introuvable
