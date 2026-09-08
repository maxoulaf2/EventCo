# language: fr
Fonctionnalité: Détail d'un événement
  En tant que participant d'un événement je veux en consulter le détail et la
  liste des participants, et en tant que créateur je veux pouvoir promouvoir ou
  rétrograder un participant en co-organisateur

  Scénario: Affichage des informations et des participants
    Quand j'arrive sur le détail de l'événement
    Alors je vois le titre "Repas de Noël" et le lieu "Chez Alice"
    Et je vois le participant "Test" avec le rôle "Co-organisateur"
    Et je vois le participant "Ami" avec le rôle "Participant" et un badge d'invitation en attente

  Scénario: Le créateur promeut un participant en co-organisateur
    Quand j'arrive sur le détail de l'événement
    Et je clique sur "Promouvoir co-organisateur" pour "Ami"
    Alors je vois le participant "Ami" avec le rôle "Co-organisateur"

  Scénario: Un participant qui n'est pas le créateur ne voit aucune action
    Etant donné que je ne suis pas le créateur de cet événement
    Quand j'arrive sur le détail de l'événement
    Alors je ne vois aucun bouton pour promouvoir ou rétrograder un participant

  Scénario: Le créateur invite un nouveau participant
    Quand j'arrive sur le détail de l'événement
    Et j'invite "nouveau@example.com" comme participant
    Alors je vois le participant "nouveau" avec le rôle "Participant" et un badge d'invitation en attente

  Scénario: Invitation d'une personne déjà invitée
    Quand j'arrive sur le détail de l'événement
    Et j'invite "ami@example.com" comme participant
    Alors je vois un message d'erreur pour l'invitation

  Scénario: Un co-organisateur non créateur peut aussi inviter un participant
    Etant donné que je suis un co-organisateur non créateur de cet événement
    Quand j'arrive sur le détail de l'événement
    Alors je vois le formulaire d'invitation

  Scénario: Un simple participant ne voit pas le formulaire d'invitation
    Etant donné que je ne suis pas le créateur de cet événement
    Quand j'arrive sur le détail de l'événement
    Alors je ne vois pas de formulaire d'invitation

  Scénario: Session expirée
    Etant donné que ma session a expiré
    Quand j'arrive sur le détail de l'événement
    Alors je suis redirigé vers la page de connexion
