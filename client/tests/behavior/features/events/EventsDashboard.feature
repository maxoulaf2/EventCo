# language: fr
Fonctionnalité: Tableau de bord des événements
  En tant qu'utilisateur connecté je veux voir la liste des événements auxquels
  je participe afin de les retrouver facilement après connexion

  Scénario: Liste de mes événements, dont un que j'organise
    Quand j'arrive sur le tableau de bord
    Alors je vois l'événement "Repas de Noël" avec un badge d'organisateur·ice
    Et je vois l'événement "Weekend au ski" sans badge d'organisateur·ice

  Scénario: Aucun événement
    Etant donné que je n'ai encore aucun événement
    Quand j'arrive sur le tableau de bord
    Alors je vois un message m'indiquant que je ne participe à aucun événement

  Scénario: Session expirée
    Etant donné que ma session a expiré
    Quand j'arrive sur le tableau de bord
    Alors je suis redirigé vers la page de connexion

  Scénario: Un administrateur voit le lien vers tous les événements
    Etant donné que je suis administrateur
    Quand j'arrive sur le tableau de bord
    Alors je vois le lien vers tous les événements

  Scénario: Un utilisateur non administrateur ne voit pas le lien vers tous les événements
    Quand j'arrive sur le tableau de bord
    Alors je ne vois pas le lien vers tous les événements
