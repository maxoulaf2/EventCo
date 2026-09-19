# language: fr
Fonctionnalité: Régénération du lien d'invitation d'un événement
  En tant qu'organisateur je veux pouvoir régénérer le lien d'invitation d'un événement
  afin d'invalider l'ancien lien s'il a été partagé par erreur

  Scénario: Le créateur régénère le lien
    Etant donné un événement à régénérer "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je régénère le lien d'invitation de cet événement
    Alors la régénération réussit
    Et le nouveau lien diffère de l'ancien
    Et l'ancien lien ne permet plus de rejoindre l'événement

  Scénario: Un co-organisateur régénère le lien
    Etant donné un événement à régénérer "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et un co-organisateur "ami@example.com" pour cet événement
    Quand je régénère le lien d'invitation de cet événement en tant que co-organisateur
    Alors la régénération réussit

  Scénario: Un simple participant tente de régénérer le lien
    Etant donné un événement à régénérer "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand je régénère le lien d'invitation de cet événement
    Alors la régénération échoue avec une erreur d'autorisation

  Scénario: Régénération sur un événement inexistant
    Quand je régénère le lien d'invitation d'un événement inexistant
    Alors la régénération échoue avec une erreur d'événement introuvable
