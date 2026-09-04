# language: fr
Fonctionnalité: Tableau de bord des événements
  En tant qu'utilisateur connecté je veux voir la liste des événements auxquels
  je participe afin de les retrouver facilement après connexion

  Scénario: Liste de mes événements, dont une invitation en attente
    Quand j'arrive sur le tableau de bord
    Alors je vois l'événement "Repas de Noël" sans badge d'invitation en attente
    Et je vois l'événement "Weekend au ski" avec un badge d'invitation en attente

  Scénario: Aucun événement
    Etant donné que je n'ai encore aucun événement
    Quand j'arrive sur le tableau de bord
    Alors je vois un message m'indiquant que je ne participe à aucun événement

  Scénario: Session expirée
    Etant donné que ma session a expiré
    Quand j'arrive sur le tableau de bord
    Alors je suis redirigé vers la page de connexion
