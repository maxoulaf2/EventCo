# language: fr
Fonctionnalité: Consultation de tous les événements par un administrateur
  En tant qu'administrateur je veux consulter la liste de tous les événements existants
  sans avoir à y participer

  Scénario: Un administrateur consulte tous les événements
    Etant donné un événement "Repas de Noël" organisé par un autre utilisateur
    Et un événement "Anniversaire de Bob" organisé par un autre utilisateur
    Et je suis connecté en tant qu'administrateur
    Quand je consulte la liste de tous les événements
    Alors la consultation de tous les événements réussit
    Et la liste de tous les événements contient "Repas de Noël" avec 1 participant
    Et la liste de tous les événements contient "Anniversaire de Bob" avec 1 participant

  Scénario: Un administrateur sans événement existant obtient une liste vide
    Etant donné je suis connecté en tant qu'administrateur
    Quand je consulte la liste de tous les événements
    Alors la consultation de tous les événements réussit
    Et la liste de tous les événements est vide

  Scénario: Un utilisateur non administrateur ne peut pas consulter tous les événements
    Etant donné un événement "Repas de Noël" organisé par un autre utilisateur
    Et je suis connecté en tant qu'utilisateur non administrateur
    Quand je consulte la liste de tous les événements
    Alors la consultation de tous les événements échoue avec une erreur d'autorisation administrateur
