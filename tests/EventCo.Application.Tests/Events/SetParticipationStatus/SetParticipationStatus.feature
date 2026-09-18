# language: fr
Fonctionnalité: Indication de participation à un événement
  En tant que participant d'un événement je veux indiquer si je viens
  afin que les autres participants sachent qui est présent

  Scénario: Indiquer que je viens
    Etant donné un événement "Repas de Noël" avec un participant invité "ami@example.com", qui participera le "2026-12-24" au lieu "Chez Alice"
    Et je suis le participant invité
    Quand j'indique le statut de participation "Attending"
    Alors le changement de statut réussit
    Et mon statut de participation est "Attending"

  Scénario: Indiquer que je ne viens pas
    Etant donné un événement "Repas de Noël" avec un participant invité "ami@example.com", qui participera le "2026-12-24" au lieu "Chez Alice"
    Et je suis le participant invité
    Quand j'indique le statut de participation "NotAttending"
    Alors le changement de statut réussit
    Et mon statut de participation est "NotAttending"

  Scénario: Statut par défaut d'un participant nouvellement invité
    Etant donné un événement "Repas de Noël" avec un participant invité "ami@example.com", qui participera le "2026-12-24" au lieu "Chez Alice"
    Alors mon statut de participation est "Unknown"

  Scénario: Le créateur ne peut pas modifier son propre statut de participation
    Etant donné un événement "Repas de Noël" avec un participant invité "ami@example.com", qui participera le "2026-12-24" au lieu "Chez Alice"
    Quand j'indique le statut de participation "NotAttending"
    Alors le changement de statut échoue avec une erreur réservée au créateur

  Scénario: Statut de participation invalide
    Etant donné un événement "Repas de Noël" avec un participant invité "ami@example.com", qui participera le "2026-12-24" au lieu "Chez Alice"
    Et je suis le participant invité
    Quand j'indique le statut de participation "Peut-être"
    Alors le changement de statut échoue avec une erreur de validation

  Scénario: Changement de statut par un utilisateur qui ne participe pas à l'événement
    Etant donné un événement "Repas de Noël" avec un participant invité "ami@example.com", qui participera le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand j'indique le statut de participation "Attending"
    Alors le changement de statut échoue avec une erreur de participant introuvable

  Scénario: Changement de statut sur un événement inexistant
    Quand j'indique le statut de participation "Attending" sur un événement inexistant
    Alors le changement de statut échoue avec une erreur d'événement introuvable
