# language: fr
Fonctionnalité: Modification du nom d'affichage via l'API
  En tant qu'utilisateur connecté je veux modifier mon nom d'affichage via l'API
  afin qu'il reflète comment je souhaite apparaître aux autres participants

  Scénario: Modification avec une session valide
    Etant donné une session ouverte via l'API pour "update-profile-api-test@example.com"
    Quand je modifie mon nom d'affichage via l'API en "Alice B."
    Alors la réponse de modification de profil a le statut 200
    Et l'utilisateur retourné a pour nom d'affichage "Alice B."

  Scénario: Modification avec un nom d'affichage vide
    Etant donné une session ouverte via l'API pour "update-profile-invalid-api-test@example.com"
    Quand je modifie mon nom d'affichage via l'API en ""
    Alors la réponse de modification de profil a le statut 400

  Scénario: Modification sans cookie de session
    Quand je modifie mon nom d'affichage via l'API en "Alice B." sans cookie de session
    Alors la réponse de modification de profil a le statut 401
