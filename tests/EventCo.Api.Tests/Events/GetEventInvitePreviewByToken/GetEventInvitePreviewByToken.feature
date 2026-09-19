# language: fr
Fonctionnalité: Aperçu d'un événement via son lien d'invitation via l'API
  En tant que visiteur non connecté je veux consulter l'aperçu d'un événement via son lien d'invitation
  afin de savoir à quoi je vais adhérer avant de me connecter

  Scénario: Aperçu accessible sans cookie de session
    Etant donné une session ouverte via l'API pour "preview-link-organizer-api-test@example.com"
    Et un événement "Repas de Noël" créé via l'API
    Quand je consulte l'aperçu de cet événement via l'API sans cookie de session
    Alors la réponse d'aperçu a le statut 200
    Et l'aperçu indique le titre "Repas de Noël"

  Scénario: Aperçu avec un lien d'invitation inexistant
    Quand je consulte l'aperçu d'un lien d'invitation inexistant via l'API
    Alors la réponse d'aperçu a le statut 404
