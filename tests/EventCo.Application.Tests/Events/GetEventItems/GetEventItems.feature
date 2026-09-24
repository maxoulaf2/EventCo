# language: fr
Fonctionnalité: Consultation des articles d'un événement
  En tant que participant d'un événement je veux consulter la liste de ses articles
  afin de savoir ce qu'il reste à faire

  Scénario: Consultation des articles d'un événement sans article
    Etant donné un événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice" dont je veux consulter les articles
    Quand je consulte les articles de cet événement
    Alors la consultation des articles réussit
    Et l'événement consulté a 0 article

  Scénario: Consultation des articles d'un événement avec plusieurs articles
    Etant donné un événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice" dont je veux consulter les articles
    Et un article "Bûche au chocolat" est ajouté à cet événement
    Et un article "Réserver la salle" est ajouté à cet événement
    Quand je consulte les articles de cet événement
    Alors la consultation des articles réussit
    Et l'événement consulté a 2 articles
    Et l'événement consulté a un article "Bûche au chocolat"

  Scénario: Consultation des articles d'un événement inexistant
    Quand je consulte les articles d'un événement inexistant
    Alors la consultation des articles échoue avec une erreur d'événement introuvable

  Scénario: Consultation des articles par un utilisateur qui n'est pas participant
    Etant donné un événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice" dont je veux consulter les articles
    Et je change d'utilisateur courant
    Quand je consulte les articles de cet événement
    Alors la consultation des articles échoue avec une erreur d'autorisation

  Scénario: Consultation des articles par un administrateur qui n'est pas participant
    Etant donné un événement "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice" dont je veux consulter les articles
    Et un article "Bûche au chocolat" est ajouté à cet événement
    Et je change d'utilisateur courant pour un administrateur
    Quand je consulte les articles de cet événement
    Alors la consultation des articles réussit
    Et l'événement consulté a 1 article
