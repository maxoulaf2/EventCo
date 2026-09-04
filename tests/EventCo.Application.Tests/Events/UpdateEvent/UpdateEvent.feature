# language: fr
Fonctionnalité: Modification d'un événement
  En tant qu'utilisateur connecté je veux modifier les informations d'un événement existant
  afin de corriger ou préciser ses détails

  Scénario: Modification avec des données valides
    Etant donné un événement existant "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je modifie cet événement avec le titre "Réveillon de Noël" prévu le "2026-12-25" au lieu "Chez Bob"
    Alors la modification réussit
    Et l'événement modifié a pour titre "Réveillon de Noël"
    Et l'événement modifié a pour date "2026-12-25"
    Et l'événement modifié a pour lieu "Chez Bob"

  Scénario: Modification avec un titre vide
    Etant donné un événement existant "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je modifie cet événement avec le titre "" prévu le "2026-12-25" au lieu "Chez Bob"
    Alors la modification échoue avec une erreur de validation

  Scénario: Modification d'un événement inexistant
    Quand je modifie un événement inexistant avec le titre "Réveillon de Noël" prévu le "2026-12-25" au lieu "Chez Bob"
    Alors la modification échoue avec une erreur d'événement introuvable
