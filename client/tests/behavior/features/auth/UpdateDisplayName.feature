# language: fr
Fonctionnalité: Modification du nom d'affichage
  En tant qu'utilisateur connecté je veux modifier mon nom d'affichage depuis la modale
  de mon compte afin qu'il reflète comment je souhaite apparaître aux autres participants

  Scénario: Modification avec un nom d'affichage valide
    Quand j'arrive sur le tableau de bord
    Et j'ouvre la modale de mon compte
    Alors le champ nom d'affichage est pré-rempli avec "Test"
    Quand je modifie le nom d'affichage en "Alice B."
    Et je valide le formulaire de nom d'affichage
    Alors je vois un message de succès

  Scénario: Le serveur refuse la modification
    Quand j'arrive sur le tableau de bord
    Et j'ouvre la modale de mon compte
    Et le serveur refuse la modification du nom d'affichage
    Et je modifie le nom d'affichage en "Alice B."
    Et je valide le formulaire de nom d'affichage
    Alors je vois un message d'erreur
