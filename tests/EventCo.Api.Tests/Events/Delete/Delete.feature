# language: fr
Fonctionnalité: Suppression d'un événement via l'API
  En tant que créateur d'un événement je veux le supprimer via l'API
  afin de retirer un événement qui n'a plus lieu d'être

  Scénario: Suppression d'un événement existant par son créateur
    Etant donné une session ouverte via l'API pour "delete-event-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand je supprime cet événement via l'API
    Alors la réponse de suppression d'événement a le statut 204

  Scénario: Suppression d'un événement inexistant
    Etant donné une session ouverte via l'API pour "delete-missing-event-api-test@example.com"
    Quand je supprime un événement inexistant via l'API
    Alors la réponse de suppression d'événement a le statut 404

  Scénario: Suppression sans cookie de session
    Quand je supprime un événement inexistant via l'API sans cookie de session
    Alors la réponse de suppression d'événement a le statut 401

  Scénario: Suppression par un utilisateur qui n'est pas le créateur
    Etant donné une session ouverte via l'API pour "delete-event-creator-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et une session ouverte via l'API pour "delete-event-other-user-api-test@example.com"
    Quand je supprime cet événement via l'API
    Alors la réponse de suppression d'événement a le statut 403
