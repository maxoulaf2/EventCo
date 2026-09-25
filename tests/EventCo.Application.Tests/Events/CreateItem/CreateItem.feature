# language: fr
Fonctionnalité: Création d'un article sur un événement
  En tant que participant d'un événement je veux ajouter ce que j'apporte,
  et en tant que créateur/co-organisateur je veux aussi ajouter ce qu'il faudrait apporter,
  afin de répartir les préparatifs

  Scénario: Création d'un article à prendre par le créateur
    Etant donné un événement ouvert à l'ajout d'articles "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'ajoute l'article à prendre "Bûche au chocolat" de quantité "1" à cet événement
    Alors la création de l'article réussit
    Et l'article créé a pour titre "Bûche au chocolat"
    Et l'article créé est de nature "ToBring"
    Et l'article créé n'est attribué à personne
    Et une notification temps réel de création d'article est diffusée

  Scénario: Création d'un article apporté par le créateur
    Etant donné un événement ouvert à l'ajout d'articles "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'ajoute l'article que j'apporte "Bûche au chocolat" de quantité "1" à cet événement
    Alors la création de l'article réussit
    Et l'article créé est de nature "Contribution"
    Et l'article créé m'est attribué

  Scénario: Création d'un article apporté par un participant simple (non organisateur)
    Etant donné un événement ouvert à l'ajout d'articles "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et un participant "ami@example.com" a rejoint cet événement
    Et j'agis désormais en tant que ce participant
    Quand j'ajoute l'article que j'apporte "Bûche au chocolat" de quantité "1" à cet événement
    Alors la création de l'article réussit
    Et l'article créé est de nature "Contribution"
    Et l'article créé m'est attribué

  Scénario: Un participant simple ne peut pas ajouter d'article à prendre
    Etant donné un événement ouvert à l'ajout d'articles "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et un participant "ami@example.com" a rejoint cet événement
    Et j'agis désormais en tant que ce participant
    Quand j'ajoute l'article à prendre "Bûche au chocolat" de quantité "1" à cet événement
    Alors la création de l'article échoue avec une erreur d'article à prendre réservé aux organisateurs

  Scénario: Création d'un article de nature inconnue
    Etant donné un événement ouvert à l'ajout d'articles "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'ajoute l'article de nature "Autre" "Bûche au chocolat" de quantité "1" à cet événement
    Alors la création de l'article échoue avec une erreur de validation

  Scénario: Création d'un article avec un titre vide
    Etant donné un événement ouvert à l'ajout d'articles "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand j'ajoute l'article que j'apporte " " de quantité "1" à cet événement
    Alors la création de l'article échoue avec une erreur de validation

  Scénario: Création d'un article sur un événement inexistant
    Quand j'ajoute l'article que j'apporte "Bûche au chocolat" de quantité "1" à un événement inexistant
    Alors la création de l'article échoue avec une erreur d'événement introuvable

  Scénario: Création d'un article par un utilisateur qui ne participe pas à l'événement
    Etant donné un événement ouvert à l'ajout d'articles "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Et je change d'utilisateur courant
    Quand j'ajoute l'article que j'apporte "Bûche au chocolat" de quantité "1" à cet événement
    Alors la création de l'article échoue avec une erreur d'autorisation
