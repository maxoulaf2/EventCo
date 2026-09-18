# language: fr
Fonctionnalité: Consultation de mes événements
  En tant qu'utilisateur connecté je veux consulter la liste des événements
  auxquels je participe afin de les retrouver facilement

  Scénario: Liste des événements dont je suis participant
    Etant donné un événement "Repas de Noël" que j'ai créé
    Et un événement "Anniversaire de Bob" créé par un autre utilisateur auquel je ne participe pas
    Quand je consulte la liste de mes événements
    Alors la consultation de mes événements réussit
    Et ma liste d'événements contient "Repas de Noël"
    Et ma liste d'événements ne contient pas "Anniversaire de Bob"

  Scénario: Un événement auquel je suis invité apparaît avec mon rôle
    Etant donné un événement "Repas de Noël" que j'ai créé
    Et un événement "Weekend au ski" créé par un autre utilisateur qui m'y a invité
    Quand je consulte la liste de mes événements
    Alors "Repas de Noël" apparaît avec le rôle "Organizer"
    Et "Weekend au ski" apparaît avec le rôle "Participant"

  Scénario: Aucun événement
    Quand je consulte la liste de mes événements
    Alors ma liste d'événements est vide
