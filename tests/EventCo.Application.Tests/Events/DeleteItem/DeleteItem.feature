# language: fr
Fonctionnalité: Suppression d'un article
  En tant que participant d'un événement je veux supprimer un article
  afin de retirer un article devenu inutile de la liste des préparatifs

  Scénario: Un participant supprime l'article qu'il a créé
    Etant donné un événement "Repas de Noël" avec un article créé par un participant invité "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je supprime cet article
    Alors la suppression de l'article réussit
    Et l'article n'existe plus
    Et une notification temps réel de suppression d'article est diffusée

  Scénario: Le créateur de l'événement supprime un article créé par un autre participant
    Etant donné un événement "Repas de Noël" avec un article créé par un participant invité "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et je redeviens le créateur de l'événement
    Quand je supprime cet article
    Alors la suppression de l'article réussit
    Et l'article n'existe plus
    Et une notification temps réel de suppression d'article est diffusée

  Scénario: Un participant simple tente de supprimer l'article créé par un autre participant
    Etant donné un événement "Repas de Noël" avec un article créé par un participant invité "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et un autre participant invité devient l'utilisateur courant
    Quand je supprime cet article
    Alors la suppression de l'article échoue avec une erreur de suppression réservée au créateur de l'article

  Scénario: Suppression par un utilisateur qui ne participe pas à l'événement
    Etant donné un événement "Repas de Noël" avec un article créé par un participant invité "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand je supprime cet article
    Alors la suppression de l'article échoue avec une erreur d'autorisation

  Scénario: Suppression sur un événement inexistant
    Quand je supprime un article sur un événement inexistant
    Alors la suppression de l'article échoue avec une erreur d'événement introuvable

  Scénario: Suppression d'un article inexistant
    Etant donné un événement "Repas de Noël" avec un article créé par un participant invité "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je supprime un article inexistant sur cet événement
    Alors la suppression de l'article échoue avec une erreur d'article introuvable
