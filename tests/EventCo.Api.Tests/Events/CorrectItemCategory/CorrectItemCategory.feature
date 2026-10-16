# language: fr
Fonctionnalité: Correction de la catégorie d'un article via l'API
  En tant qu'utilisateur connecté je veux pouvoir corriger la catégorie déduite d'un article via l'API
  afin que la liste des articles reste bien rangée

  Scénario: Correction réussie par la personne qui apporte l'article
    Etant donné une session ouverte via l'API pour "correct-category-contributor-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et "correct-category-contributor-guest-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "correct-category-contributor-guest-api-test@example.com"
    Et un article apporté "Le truc de mamie" ajouté à cet événement via l'API
    Quand je corrige la catégorie de cet article en "Desserts" via l'API
    Alors la réponse de correction de catégorie a le statut 204
    Quand je consulte les articles de cet événement via l'API
    Alors l'article consulté retourné "Le truc de mamie" est dans la catégorie "Desserts"

  Scénario: Correction refusée pour un participant qui n'a pas créé l'article
    Etant donné une session ouverte via l'API pour "correct-category-others-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un article à prendre "Bûche au chocolat" ajouté à cet événement via l'API
    Et "correct-category-others-guest-api-test@example.com" invité à cet événement via l'API
    Et une session ouverte via l'API pour "correct-category-others-guest-api-test@example.com"
    Quand je corrige la catégorie de cet article en "Drinks" via l'API
    Alors la réponse de correction de catégorie a le statut 403

  Scénario: Correction vers une catégorie inconnue
    Etant donné une session ouverte via l'API pour "correct-category-unknown-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Et un article à prendre "Bûche au chocolat" ajouté à cet événement via l'API
    Quand je corrige la catégorie de cet article en "Plats" via l'API
    Alors la réponse de correction de catégorie a le statut 400

  Scénario: Correction sur un événement inexistant
    Etant donné une session ouverte via l'API pour "correct-category-missing-event-api-test@example.com"
    Quand je corrige la catégorie d'un article sur un événement inexistant via l'API
    Alors la réponse de correction de catégorie a le statut 404

  Scénario: Correction sans cookie de session
    Quand je corrige la catégorie d'un article sur un événement inexistant via l'API sans cookie de session
    Alors la réponse de correction de catégorie a le statut 401
