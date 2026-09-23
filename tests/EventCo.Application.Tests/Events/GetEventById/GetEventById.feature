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

  Scénario: Consultation par un utilisateur qui n'est pas participant
    Etant donné un événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand je consulte cet événement
    Alors la consultation échoue avec une erreur d'autorisation

  Scénario: Consultation d'un événement avec ses participants
    Etant donné un événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et "ami@example.com" est invité à cet événement
    Quand je consulte cet événement
    Alors la consultation réussit
    Et l'événement consulté a 2 participants
    Et l'événement consulté a un participant "ami@example.com" avec le rôle "Participant" sans statut de participation indiqué

  Scénario: Consultation par un administrateur qui n'est pas participant
    Etant donné un événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant pour un administrateur
    Quand je consulte cet événement
    Alors la consultation réussit
    Et l'événement consulté a pour titre "Repas de Noël"
    Et l'utilisateur courant ne figure pas parmi les participants de l'événement consulté
    Et le lien d'invitation de l'événement consulté n'est pas fourni
