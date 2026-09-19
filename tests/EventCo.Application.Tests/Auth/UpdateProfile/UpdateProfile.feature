# language: fr
Fonctionnalité: Modification du nom d'affichage
  En tant qu'utilisateur connecté je veux modifier mon nom d'affichage
  afin qu'il reflète comment je souhaite apparaître aux autres participants

  Scénario: Modification avec un nom d'affichage valide
    Etant donné un compte existant pour "alice@example.com"
    Quand je modifie mon nom d'affichage en "Alice B."
    Alors la modification du nom d'affichage réussit
    Et mon nom d'affichage est "Alice B."

  Scénario: Modification avec un nom d'affichage vide
    Etant donné un compte existant pour "alice@example.com"
    Quand je modifie mon nom d'affichage en ""
    Alors la modification du nom d'affichage échoue avec une erreur de validation
