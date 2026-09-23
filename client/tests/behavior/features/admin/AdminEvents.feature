# language: fr
Fonctionnalité: Consultation de tous les événements par un administrateur
  En tant qu'administrateur je veux voir la liste de tous les événements existants
  afin de pouvoir les consulter sans y participer

  Scénario: Liste de tous les événements
    Quand j'arrive sur la page d'administration des événements
    Alors je vois l'événement "Repas de Noël" avec 2 participants
    Et je vois l'événement "Anniversaire de Bob" avec 1 participant

  Scénario: Ouverture du détail d'un événement depuis la liste
    Quand j'arrive sur la page d'administration des événements
    Et je clique sur l'événement "Repas de Noël"
    Alors je vois le détail de l'événement

  Scénario: Aucun événement existant
    Etant donné qu'aucun événement n'existe
    Quand j'arrive sur la page d'administration des événements
    Alors je vois un message m'indiquant qu'aucun événement n'existe

  Scénario: Accès refusé à un utilisateur non administrateur
    Etant donné que je ne suis pas administrateur
    Quand j'arrive sur la page d'administration des événements
    Alors je vois un message m'indiquant que la page est réservée aux administrateurs
