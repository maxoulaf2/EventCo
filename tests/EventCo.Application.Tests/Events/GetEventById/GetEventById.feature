# language: fr
Fonctionnalité: Consultation d'un événement
  En tant qu'utilisateur connecté je veux consulter un événement existant
  afin d'en voir les détails

  Scénario: Consultation d'un événement existant
    Etant donné un événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je consulte cet événement
    Alors la consultation réussit
    Et l'événement consulté a pour titre "Repas de Noël"

  Scénario: Consultation d'un événement inexistant
    Quand je consulte un événement inexistant
    Alors la consultation échoue avec une erreur d'événement introuvable
