# language: fr
Fonctionnalité: Suppression d'une tâche
  En tant que participant d'un événement je veux supprimer une tâche
  afin de retirer une tâche devenue inutile de la liste des préparatifs

  Scénario: Un participant supprime la tâche qu'il a créée
    Etant donné un événement "Repas de Noël" avec une tâche créée par un participant invité "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je supprime cette tâche
    Alors la suppression de la tâche réussit
    Et la tâche n'existe plus
    Et une notification temps réel de suppression de tâche est diffusée

  Scénario: Le créateur de l'événement supprime une tâche créée par un autre participant
    Etant donné un événement "Repas de Noël" avec une tâche créée par un participant invité "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et je redeviens le créateur de l'événement
    Quand je supprime cette tâche
    Alors la suppression de la tâche réussit
    Et la tâche n'existe plus
    Et une notification temps réel de suppression de tâche est diffusée

  Scénario: Un participant simple tente de supprimer la tâche créée par un autre participant
    Etant donné un événement "Repas de Noël" avec une tâche créée par un participant invité "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et un autre participant invité devient l'utilisateur courant
    Quand je supprime cette tâche
    Alors la suppression de la tâche échoue avec une erreur de suppression réservée au créateur de la tâche

  Scénario: Suppression par un utilisateur qui ne participe pas à l'événement
    Etant donné un événement "Repas de Noël" avec une tâche créée par un participant invité "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand je supprime cette tâche
    Alors la suppression de la tâche échoue avec une erreur d'autorisation

  Scénario: Suppression sur un événement inexistant
    Quand je supprime une tâche sur un événement inexistant
    Alors la suppression de la tâche échoue avec une erreur d'événement introuvable

  Scénario: Suppression d'une tâche inexistante
    Etant donné un événement "Repas de Noël" avec une tâche créée par un participant invité "Bûche au chocolat", prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je supprime une tâche inexistante sur cet événement
    Alors la suppression de la tâche échoue avec une erreur de tâche introuvable
