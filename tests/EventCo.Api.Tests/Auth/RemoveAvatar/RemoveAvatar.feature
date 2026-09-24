# language: fr
Fonctionnalité: Suppression de la photo de profil via l'API
  En tant qu'utilisateur connecté je veux retirer ma photo de profil via l'API
  afin de revenir à l'avatar par défaut (initiale de mon nom)

  Scénario: Suppression d'une photo existante
    Etant donné une session ouverte via l'API pour "remove-avatar-api-test@example.com"
    Et une photo de profil déjà envoyée via l'API
    Quand je supprime ma photo de profil via l'API
    Alors la réponse de suppression de photo de profil a le statut 200
    Et l'utilisateur retourné n'a plus de photo de profil
    Et l'ancienne photo de profil n'est plus servie

  Scénario: Suppression sans cookie de session
    Quand je supprime ma photo de profil via l'API sans cookie de session
    Alors la réponse de suppression de photo de profil a le statut 401
