# language: fr
Fonctionnalité: Désassignation d'un article
  En tant que participant d'un événement je veux pouvoir me désassigner d'un article
  afin de la rendre disponible pour quelqu'un d'autre si je ne peux plus m'en occuper

  Scénario: Auto-désassignation par le participant assigné
    Etant donné un événement "Repas de Noël" avec un article "Bûche au chocolat" déjà assigné à un participant, prévu le "2026-12-24" au lieu "Chez Alice"
    Et je deviens ce participant assigné
    Quand je désassigne cet article
    Alors la désassignation réussit
    Et l'article n'est plus assigné
    Et une notification temps réel de désassignation d'article est diffusée

  Scénario: Désassignation par le créateur d'un article assigné à un autre participant
    Etant donné un événement "Repas de Noël" avec un article "Bûche au chocolat" déjà assigné à un participant, prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je désassigne cet article
    Alors la désassignation réussit
    Et l'article n'est plus assigné

  Scénario: Désassignation refusée pour l'article assigné à un autre participant
    Etant donné un événement "Repas de Noël" avec un article "Bûche au chocolat" déjà assigné à un participant, prévu le "2026-12-24" au lieu "Chez Alice"
    Et un second participant a rejoint cet événement et devient l'utilisateur courant
    Quand je désassigne cet article
    Alors la désassignation échoue avec une erreur de désassignation réservée à l'assigné

  Scénario: Désassignation par un utilisateur qui ne participe pas à l'événement
    Etant donné un événement "Repas de Noël" avec un article "Bûche au chocolat" déjà assigné à un participant, prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand je désassigne cet article
    Alors la désassignation échoue avec une erreur d'autorisation

  Scénario: Désassignation sur un événement inexistant
    Quand je désassigne un article sur un événement inexistant
    Alors la désassignation échoue avec une erreur d'événement introuvable

  Scénario: Désassignation d'un article inexistant
    Etant donné un événement "Repas de Noël" avec un article "Bûche au chocolat" déjà assigné à un participant, prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je désassigne un article inexistant sur cet événement
    Alors la désassignation échoue avec une erreur d'article introuvable
