# language: fr
Fonctionnalité: Création d'un événement
  En tant qu'utilisateur connecté je veux créer un événement
  afin de co-organiser un événement de groupe avec mes amis

  Scénario: Création avec des données valides
    Quand je crée l'événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Alors la création réussit
    Et l'événement créé a pour titre "Repas de Noël"
    Et je suis inscrit comme organisateur de l'événement créé

  Scénario: Création avec un titre vide
    Quand je crée l'événement "" prévu le "2026-12-24" au lieu "Chez Alice"
    Alors la création échoue avec une erreur de validation

  Scénario: Création avec une image
    Quand je crée l'événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice" avec l'image "https://example.com/photo.jpg"
    Alors la création réussit
    Et l'événement créé a pour image "https://example.com/photo.jpg"

  Scénario: Création sans image
    Quand je crée l'événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Alors la création réussit
    Et l'événement créé n'a pas d'image
