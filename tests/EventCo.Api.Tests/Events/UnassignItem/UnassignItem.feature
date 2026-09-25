# language: fr
Fonctionnalité: Désassignation d'un article via l'API
  En tant qu'utilisateur connecté je veux pouvoir désassigner un article via l'API
  afin de la rendre disponible pour quelqu'un d'autre si je ne peux plus m'en occuper

  Scénario: Désassignation réussie par le participant assigné
    Etant donné une session ouverte via l'API pour "unassign-item-assignee-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un article à prendre "Bûche au chocolat" ajouté à cet événement via l'API
    Et "unassign-item-assignee-guest-api-test@example.com" invité à cet événement via l'API
    Et cet article assigné à ce participant via l'API
    Et une session ouverte via l'API pour "unassign-item-assignee-guest-api-test@example.com"
    Quand je désassigne cet article via l'API
    Alors la réponse de désassignation a le statut 204

  Scénario: Désassignation réussie par le créateur pour un article assigné à un autre participant
    Etant donné une session ouverte via l'API pour "unassign-item-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un article à prendre "Bûche au chocolat" ajouté à cet événement via l'API
    Et "unassign-item-guest-api-test@example.com" invité à cet événement via l'API
    Et cet article assigné à ce participant via l'API
    Quand je désassigne cet article via l'API
    Alors la réponse de désassignation a le statut 204

  Scénario: Désassignation refusée pour l'article assigné à un autre participant
    Etant donné une session ouverte via l'API pour "unassign-item-others-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un article à prendre "Bûche au chocolat" ajouté à cet événement via l'API
    Et "unassign-item-others-guest-api-test@example.com" invité à cet événement via l'API
    Et cet article assigné à ce participant via l'API
    Et "unassign-item-others-other-guest-api-test@example.com" également invité à cet événement via l'API
    Et une session ouverte via l'API pour "unassign-item-others-other-guest-api-test@example.com"
    Quand je désassigne cet article via l'API
    Alors la réponse de désassignation a le statut 403

  Scénario: Désassignation sur un événement inexistant
    Etant donné une session ouverte via l'API pour "unassign-item-missing-event-api-test@example.com"
    Quand je désassigne un article sur un événement inexistant via l'API
    Alors la réponse de désassignation a le statut 404

  Scénario: Désassignation sans cookie de session
    Quand je désassigne un article sur un événement inexistant via l'API sans cookie de session
    Alors la réponse de désassignation a le statut 401

  Scénario: Désassignation refusée pour un article apporté, y compris à la personne qui l'apporte
    Etant donné une session ouverte via l'API pour "unassign-item-contribution-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "unassign-item-contribution-guest-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "unassign-item-contribution-guest-api-test@example.com"
    Et un article apporté "Bûche au chocolat" ajouté à cet événement via l'API
    Quand je désassigne cet article via l'API
    Alors la réponse de désassignation a le statut 403
