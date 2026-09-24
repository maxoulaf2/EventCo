# language: fr
Fonctionnalité: Suppression d'un article via l'API
  En tant qu'utilisateur connecté je veux supprimer un article via l'API
  afin de retirer un article devenu inutile de la liste des préparatifs

  Scénario: Suppression réussie par le participant qui a créé l'article
    Etant donné une session ouverte via l'API pour "delete-item-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "delete-item-creator-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "delete-item-creator-api-test@example.com"
    Et un article "Bûche au chocolat" ajouté à cet événement via l'API
    Quand je supprime cet article via l'API
    Alors la réponse de suppression d'article a le statut 204

  Scénario: Suppression réussie par le créateur de l'événement sur un article créé par un autre participant
    Etant donné une session ouverte via l'API pour "delete-item-organizer2-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "delete-item-creator2-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "delete-item-creator2-api-test@example.com"
    Et un article "Bûche au chocolat" ajouté à cet événement via l'API
    Et une session ouverte via l'API pour "delete-item-organizer2-api-test@example.com"
    Quand je supprime cet article via l'API
    Alors la réponse de suppression d'article a le statut 204

  Scénario: Suppression refusée pour un participant qui n'a pas créé l'article
    Etant donné une session ouverte via l'API pour "delete-item-organizer3-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "delete-item-creator3-api-test@example.com" invité à cet événement via l'API
    Et "delete-item-other3-api-test@example.com" également invité à cet événement via l'API
    Et une session ouverte via l'API pour "delete-item-creator3-api-test@example.com"
    Et un article "Bûche au chocolat" ajouté à cet événement via l'API
    Et une session ouverte via l'API pour "delete-item-other3-api-test@example.com"
    Quand je supprime cet article via l'API
    Alors la réponse de suppression d'article a le statut 403

  Scénario: Suppression sur un événement inexistant
    Etant donné une session ouverte via l'API pour "delete-item-missing-event-api-test@example.com"
    Quand je supprime un article sur un événement inexistant via l'API
    Alors la réponse de suppression d'article a le statut 404

  Scénario: Suppression sans cookie de session
    Quand je supprime un article sur un événement inexistant via l'API sans cookie de session
    Alors la réponse de suppression d'article a le statut 401

  Scénario: Suppression d'un article inexistant sur un événement existant
    Etant donné une session ouverte via l'API pour "delete-item-missing-item-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand je supprime un article inexistant sur cet événement via l'API
    Alors la réponse de suppression d'article a le statut 400

  Scénario: Suppression par un utilisateur qui ne participe pas du tout à l'événement
    Etant donné une session ouverte via l'API pour "delete-item-organizer4-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un article "Bûche au chocolat" ajouté à cet événement via l'API
    Et une session ouverte via l'API pour "delete-item-non-participant4-api-test@example.com"
    Quand je supprime cet article via l'API
    Alors la réponse de suppression d'article a le statut 403
