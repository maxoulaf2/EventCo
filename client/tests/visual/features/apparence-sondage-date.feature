# language: fr
Fonctionnalité: Non-régression visuelle du sondage de date

  Scénario: Formulaire de création avec plusieurs dates proposées
    Etant donné que je suis sur le formulaire de création d'événement avec plusieurs dates proposées
    Alors son apparence correspond à la référence enregistrée

  Scénario: Tableau de bord avec un événement en cours de vote
    Etant donné que je suis sur le tableau de bord avec un événement en cours de vote
    Alors son apparence correspond à la référence enregistrée

  Scénario: Sondage de date vu par un organisateur
    Etant donné que je suis sur le détail d'un événement en cours de vote en tant qu'organisateur
    Alors son apparence correspond à la référence enregistrée

  Scénario: Sondage de date vu par un simple participant
    Etant donné que je suis sur le détail d'un événement en cours de vote en tant que simple participant
    Alors son apparence correspond à la référence enregistrée
