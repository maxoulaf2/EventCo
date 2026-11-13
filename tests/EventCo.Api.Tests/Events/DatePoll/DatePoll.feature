# language: fr
Fonctionnalité: Sondage de date via l'API
  En tant qu'organisateur je veux proposer plusieurs dates au vote via l'API
  afin de retenir celle qui convient au plus grand nombre

  Scénario: Création d'un événement avec un sondage de date
    Etant donné une session ouverte via l'API pour "date-poll-create-api-test@example.com"
    Quand je crée un événement avec un sondage proposant des dates dans "10, 11" jours via l'API
    Alors la réponse du sondage a le statut 201
    Et le détail de l'événement n'a pas de date et propose 2 options

  Scénario: Un sondage avec une seule option est refusé
    Etant donné une session ouverte via l'API pour "date-poll-single-option-api-test@example.com"
    Quand je crée un événement avec un sondage proposant des dates dans "10" jours via l'API
    Alors la réponse du sondage a le statut 400

  Scénario: Un participant vote puis l'organisateur retient la date
    Etant donné une session ouverte via l'API pour "date-poll-organizer-api-test@example.com"
    Et un événement avec un sondage proposant des dates dans "10, 11" jours via l'API
    Et "date-poll-guest-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "date-poll-guest-api-test@example.com"
    Et je vote pour la date dans 11 jours via l'API
    Et le détail de l'événement montre 1 vote pour la date dans 11 jours
    Et une session ouverte via l'API pour "date-poll-organizer-api-test@example.com"
    Quand je retiens la date dans 11 jours via l'API
    Alors la réponse du sondage a le statut 204
    Et le détail de l'événement a pour date celle dans 11 jours
    Et le participant invité est indiqué présent dans le détail de l'événement

  Scénario: Un participant retire son vote
    Etant donné une session ouverte via l'API pour "date-poll-withdraw-api-test@example.com"
    Et un événement avec un sondage proposant des dates dans "10, 11" jours via l'API
    Et je vote pour la date dans 10 jours via l'API
    Quand je retire mon vote pour la date dans 10 jours via l'API
    Alors la réponse du sondage a le statut 204
    Et le détail de l'événement montre 0 vote pour la date dans 10 jours

  Scénario: Un organisateur ajoute puis supprime une option qui a reçu un vote
    Etant donné une session ouverte via l'API pour "date-poll-options-api-test@example.com"
    Et un événement avec un sondage proposant des dates dans "10, 11" jours via l'API
    Et j'ajoute une option de date dans 12 jours via l'API
    Et je vote pour la date dans 12 jours via l'API
    Quand je supprime l'option de date dans 12 jours via l'API
    Alors la réponse du sondage a le statut 204
    Et le détail de l'événement n'a pas de date et propose 2 options

  Scénario: Un participant simple ne peut pas retenir une date
    Etant donné une session ouverte via l'API pour "date-poll-forbidden-organizer-api-test@example.com"
    Et un événement avec un sondage proposant des dates dans "10, 11" jours via l'API
    Et "date-poll-forbidden-guest-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "date-poll-forbidden-guest-api-test@example.com"
    Quand je retiens la date dans 10 jours via l'API
    Alors la réponse du sondage a le statut 403

  Scénario: Vote pour une option de date inexistante
    Etant donné une session ouverte via l'API pour "date-poll-unknown-option-api-test@example.com"
    Et un événement avec un sondage proposant des dates dans "10, 11" jours via l'API
    Quand je vote pour une option de date inexistante via l'API
    Alors la réponse du sondage a le statut 400

  Scénario: Vote sans cookie de session
    Quand je vote pour une option de date sans cookie de session
    Alors la réponse du sondage a le statut 401

  Scénario: Suppression d'un événement dont le sondage a reçu des votes
    Etant donné une session ouverte via l'API pour "date-poll-delete-event-api-test@example.com"
    Et un événement avec un sondage proposant des dates dans "10, 11" jours via l'API
    Et je vote pour la date dans 10 jours via l'API
    Quand je supprime cet événement en sondage via l'API
    Alors la réponse du sondage a le statut 204

  Scénario: Un événement en cours de vote est listé avant les événements datés
    Etant donné une session ouverte via l'API pour "date-poll-list-order-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un événement avec un sondage proposant des dates dans "10, 11" jours via l'API
    Quand je consulte mes événements après le sondage via l'API
    Alors le premier événement listé n'a pas de date

  Scénario: Un vote est diffusé au groupe temps réel
    Etant donné une session ouverte via l'API pour "date-poll-realtime-api-test@example.com"
    Et un événement avec un sondage proposant des dates dans "10, 11" jours via l'API
    Et je me connecte au hub temps réel et que je rejoins cet événement
    Et j'écoute les changements du sondage en temps réel
    Quand je vote pour la date dans 10 jours via l'API
    Alors un changement du sondage est reçu en temps réel
