# language: fr
Fonctionnalité: Sondage de date
  En tant qu'organisateur je veux proposer plusieurs dates au vote
  afin de retenir celle qui convient au plus grand nombre

  Scénario: Création d'un événement avec un sondage de date
    Quand je crée un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Alors l'action sur le sondage réussit
    Et l'événement en sondage n'a pas encore de date
    Et le sondage propose 2 options

  Scénario: Un sondage de date demande au moins deux options
    Quand je crée un événement avec un sondage proposant les dates "2026-06-01"
    Alors l'action sur le sondage échoue avec l'erreur "DateOptionCountOutOfRangeException"

  Scénario: Un événement ne peut pas avoir à la fois une date fixe et un sondage
    Quand je crée un événement avec la date "2026-06-01" et un sondage proposant les dates "2026-06-02, 2026-06-03"
    Alors l'action sur le sondage échoue avec l'erreur "ValidationException"

  Scénario: Un événement sans date ni sondage est refusé
    Quand je crée un événement sans date ni sondage
    Alors l'action sur le sondage échoue avec l'erreur "ValidationException"

  Scénario: Un participant vote pour plusieurs dates
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Et un participant "ami@example.com" invité au sondage
    Et je suis le participant invité au sondage
    Quand je vote pour la date "2026-06-01"
    Et je vote pour la date "2026-06-02"
    Alors l'action sur le sondage réussit
    Et la date "2026-06-01" a 1 vote
    Et la date "2026-06-02" a 1 vote
    Et un changement du sondage est diffusé en temps réel

  Scénario: Un participant retire son vote
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Et je vote pour la date "2026-06-01"
    Quand je retire mon vote pour la date "2026-06-01"
    Alors l'action sur le sondage réussit
    Et la date "2026-06-01" a 0 vote

  Scénario: Un utilisateur qui ne participe pas à l'événement ne peut pas voter
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Et je change d'utilisateur courant
    Quand je vote pour la date "2026-06-01"
    Alors l'action sur le sondage échoue avec l'erreur "UserNotEventParticipantException"

  Scénario: Un organisateur ajoute une option de date
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Quand j'ajoute l'option de date "2026-06-03"
    Alors l'action sur le sondage réussit
    Et le sondage propose 3 options

  Scénario: Un participant simple ne peut pas ajouter d'option de date
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Et un participant "ami@example.com" invité au sondage
    Et je suis le participant invité au sondage
    Quand j'ajoute l'option de date "2026-06-03"
    Alors l'action sur le sondage échoue avec l'erreur "UserNotEventOrganizerException"

  Scénario: Une option de date déjà proposée est refusée
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Quand j'ajoute l'option de date "2026-06-02"
    Alors l'action sur le sondage échoue avec l'erreur "DateOptionDuplicateException"

  Scénario: Supprimer une option supprime aussi ses votes
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02, 2026-06-03"
    Et je vote pour la date "2026-06-03"
    Quand je supprime l'option de date "2026-06-03"
    Alors l'action sur le sondage réussit
    Et le sondage propose 2 options

  Scénario: Il reste toujours au moins deux options
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Quand je supprime l'option de date "2026-06-02"
    Alors l'action sur le sondage échoue avec l'erreur "DateOptionCountOutOfRangeException"

  Scénario: Retenir une date clôt le sondage et confirme la présence de ses votants
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Et un participant "ami@example.com" invité au sondage
    Et je suis le participant invité au sondage
    Et je vote pour la date "2026-06-02"
    Et je suis l'organisateur du sondage
    Quand je retiens la date "2026-06-02"
    Alors l'action sur le sondage réussit
    Et l'événement en sondage a pour date "2026-06-02"
    Et le participant invité au sondage a le statut de participation "Attending"

  Scénario: Les participants qui n'ont pas voté pour la date retenue restent sans réponse
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Et un participant "ami@example.com" invité au sondage
    Et je suis le participant invité au sondage
    Et je vote pour la date "2026-06-01"
    Et je suis l'organisateur du sondage
    Quand je retiens la date "2026-06-02"
    Alors le participant invité au sondage a le statut de participation "Unknown"

  Scénario: Un participant simple ne peut pas retenir une date
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Et un participant "ami@example.com" invité au sondage
    Et je suis le participant invité au sondage
    Quand je retiens la date "2026-06-01"
    Alors l'action sur le sondage échoue avec l'erreur "UserNotEventOrganizerException"

  Scénario: On ne vote plus une fois le sondage clos
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Et je retiens la date "2026-06-01"
    Quand je vote pour la date "2026-06-02"
    Alors l'action sur le sondage échoue avec l'erreur "DatePollNotOpenException"

  Scénario: Les autres informations restent modifiables pendant le sondage
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Quand je renomme l'événement en sondage "Week-end à la mer"
    Alors l'action sur le sondage réussit
    Et l'événement en sondage n'a pas encore de date

  Scénario: La date ne se fixe pas à la main pendant le sondage
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Quand je fixe à la main la date "2026-06-05" de l'événement en sondage
    Alors l'action sur le sondage échoue avec l'erreur "EventDateUnderPollException"

  Scénario: L'email d'invitation annonce le vote
    Etant donné un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Quand j'invite "ami@example.com" au sondage
    Alors l'email d'invitation annonce que la date est soumise au vote

  Scénario: Un événement en cours de vote est listé avant les événements datés
    Etant donné un événement à date fixe "2026-03-01"
    Et un événement avec un sondage proposant les dates "2026-06-01, 2026-06-02"
    Quand je consulte mes événements
    Alors le premier de mes événements n'a pas de date
