# language: fr
Fonctionnalité: Correction de la catégorie d'un article
  En tant que participant d'un événement je veux pouvoir corriger la catégorie
  déduite automatiquement d'un article afin que la liste reste bien rangée

  Scénario: Le créateur de l'événement corrige la catégorie d'un article apporté par un participant
    Etant donné un événement "Repas de Noël" avec un article "Le truc de mamie" apporté par un participant dont je veux corriger la catégorie
    Quand je corrige la catégorie de cet article en "Desserts"
    Alors la correction de catégorie réussit
    Et la catégorie de cet article est "Desserts"
    Et une notification temps réel de correction de catégorie d'article est diffusée

  Scénario: La personne qui apporte un article corrige sa catégorie
    Etant donné un événement "Repas de Noël" avec un article "Gâteau du chef" apporté par un participant dont je veux corriger la catégorie
    Et je deviens la personne qui apporte cet article
    Quand je corrige la catégorie de cet article en "Aperitif"
    Alors la correction de catégorie réussit
    Et la catégorie de cet article est "Aperitif"

  Scénario: Correction refusée pour un participant qui n'a pas créé l'article
    Etant donné un événement "Repas de Noël" avec un article "Le truc de mamie" apporté par un participant dont je veux corriger la catégorie
    Et je deviens un autre participant de cet événement
    Quand je corrige la catégorie de cet article en "Desserts"
    Alors la correction de catégorie échoue avec une erreur de correction réservée au créateur de l'article et aux organisateurs
    Et la catégorie de cet article est "Other"

  Scénario: Correction par un utilisateur qui ne participe pas à l'événement
    Etant donné un événement "Repas de Noël" avec un article "Le truc de mamie" apporté par un participant dont je veux corriger la catégorie
    Et je change d'utilisateur courant
    Quand je corrige la catégorie de cet article en "Desserts"
    Alors la correction de catégorie échoue avec une erreur d'autorisation

  Scénario: Correction vers une catégorie inconnue
    Etant donné un événement "Repas de Noël" avec un article "Le truc de mamie" apporté par un participant dont je veux corriger la catégorie
    Quand je corrige la catégorie de cet article en "Plats"
    Alors la correction de catégorie échoue avec une erreur de validation

  Scénario: Correction sur un événement inexistant
    Quand je corrige la catégorie d'un article sur un événement inexistant
    Alors la correction de catégorie échoue avec une erreur d'événement introuvable

  Scénario: Correction d'un article inexistant
    Etant donné un événement "Repas de Noël" avec un article "Le truc de mamie" apporté par un participant dont je veux corriger la catégorie
    Quand je corrige la catégorie d'un article inexistant sur cet événement
    Alors la correction de catégorie échoue avec une erreur d'article introuvable
