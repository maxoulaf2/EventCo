# language: fr
Fonctionnalité: Consultation de mes événements (E2E nominal)
  Parcours contre la vraie API et une vraie base de données, sans mock.

  Scénario: Un utilisateur retrouve sur son tableau de bord un événement auquel il participe
    Etant donné que je me connecte avec un lien magique
    Et un événement "Repas de Noël" créé via l'API pour moi
    Quand je retourne sur le tableau de bord
    Alors je vois "Repas de Noël" dans la liste de mes événements

  Scénario: Un utilisateur sans événement voit un message adapté
    Etant donné que je me connecte avec un lien magique
    Alors je vois un message m'indiquant que je ne participe à aucun événement
