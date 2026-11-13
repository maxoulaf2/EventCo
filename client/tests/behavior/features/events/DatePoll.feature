# language: fr
Fonctionnalité: Sondage de date
  En tant qu'organisateur je veux proposer plusieurs dates au vote
  afin de retenir celle qui convient au plus grand nombre

  # Dates et heures saisies en heure locale (Europe/Paris dans les tests), envoyées au serveur en UTC.
  Scénario: Création d'un événement avec plusieurs dates proposées
    Etant donné la page de création d'événement
    Quand je choisis de proposer plusieurs dates
    Et je saisis le titre "Week-end" et les dates "2026-12-24" et "2026-12-27" à "19:30"
    Et je valide la création
    Alors la requête de création contient les dates proposées "2026-12-23T23:00:00.000Z" et "2026-12-27T18:30:00.000Z" sans date fixe
    Et je suis redirigé vers le tableau de bord

  Scénario: Le formulaire de création propose une date fixe par défaut
    Etant donné la page de création d'événement
    Alors je vois le champ de date fixe et aucune date proposée

  Scénario: Un événement en cours de vote sur le tableau de bord
    Etant donné un de mes événements en cours de vote
    Quand j'arrive sur le tableau de bord
    Alors l'événement en cours de vote porte la mention "Date en cours de vote"

  Scénario: Un participant voit le sondage à la place de sa réponse de présence
    Etant donné un événement en cours de vote où je suis simple participant
    Quand j'ouvre le détail de l'événement
    Alors je vois "Date en cours de vote" à la place de la date
    Et je vois les 2 dates proposées avec leur nombre de votes
    Et la date qui a le plus de votes est mise en avant
    Et je ne peux pas indiquer ma présence
    Et je ne vois aucune action d'organisateur sur le sondage

  Scénario: Un participant vote pour une date
    Etant donné un événement en cours de vote où je suis simple participant
    Quand j'ouvre le détail de l'événement
    Et je coche la deuxième date
    Alors mon vote pour la deuxième date est envoyé
    Et la deuxième date compte 1 vote

  Scénario: Un participant retire son vote
    Etant donné un événement en cours de vote où je suis simple participant
    Quand j'ouvre le détail de l'événement
    Et je décoche la première date
    Alors le retrait de mon vote pour la première date est envoyé
    Et la première date compte 0 vote

  Scénario: Un organisateur retient une date
    Etant donné un événement en cours de vote que j'organise
    Quand j'ouvre le détail de l'événement
    Et je retiens la première date en confirmant
    Alors la clôture du sondage sur la première date est envoyée
    Et le sondage disparaît au profit de la date retenue

  Scénario: Un organisateur renonce à retenir une date
    Etant donné un événement en cours de vote que j'organise
    Quand j'ouvre le détail de l'événement
    Et je retiens la première date sans confirmer
    Alors aucune clôture du sondage n'est envoyée

  Scénario: Un organisateur ajoute une date au sondage
    Etant donné un événement en cours de vote que j'organise
    Quand j'ouvre le détail de l'événement
    Et j'ajoute la date "2027-06-03" à "20:00" au sondage
    Alors l'ajout de la date "2027-06-03T18:00:00.000Z" est envoyé
    Et je vois 3 dates proposées
    Et je peux supprimer une date proposée

  Scénario: Le serveur refuse une action sur le sondage
    Etant donné un événement en cours de vote que j'organise
    Et le serveur refusera le prochain vote
    Quand j'ouvre le détail de l'événement
    Et je coche la deuxième date
    Alors je vois le message d'erreur du sondage
