# language: fr
Fonctionnalité: Désassignation d'une tâche
  En tant que participant d'un événement je veux pouvoir me désassigner d'une tâche
  afin de la rendre disponible pour quelqu'un d'autre si je ne peux plus m'en occuper

  Scénario: Auto-désassignation par le participant assigné
    Etant donné un événement "Repas de Noël" avec une tâche "Bûche au chocolat" déjà assignée à un participant, prévu le "2026-12-24" au lieu "Chez Alice"
    Et je deviens ce participant assigné
    Quand je désassigne cette tâche
    Alors la désassignation réussit
    Et la tâche n'est plus assignée
    Et une notification temps réel de désassignation de tâche est diffusée

  Scénario: Désassignation par le créateur d'une tâche assignée à un autre participant
    Etant donné un événement "Repas de Noël" avec une tâche "Bûche au chocolat" déjà assignée à un participant, prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je désassigne cette tâche
    Alors la désassignation réussit
    Et la tâche n'est plus assignée

  Scénario: Désassignation refusée pour la tâche assignée à un autre participant
    Etant donné un événement "Repas de Noël" avec une tâche "Bûche au chocolat" déjà assignée à un participant, prévu le "2026-12-24" au lieu "Chez Alice"
    Et un second participant a rejoint cet événement et devient l'utilisateur courant
    Quand je désassigne cette tâche
    Alors la désassignation échoue avec une erreur de désassignation réservée à l'assigné

  Scénario: Désassignation par un utilisateur qui ne participe pas à l'événement
    Etant donné un événement "Repas de Noël" avec une tâche "Bûche au chocolat" déjà assignée à un participant, prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand je désassigne cette tâche
    Alors la désassignation échoue avec une erreur d'autorisation

  Scénario: Désassignation sur un événement inexistant
    Quand je désassigne une tâche sur un événement inexistant
    Alors la désassignation échoue avec une erreur d'événement introuvable

  Scénario: Désassignation d'une tâche inexistante
    Etant donné un événement "Repas de Noël" avec une tâche "Bûche au chocolat" déjà assignée à un participant, prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je désassigne une tâche inexistante sur cet événement
    Alors la désassignation échoue avec une erreur de tâche introuvable
