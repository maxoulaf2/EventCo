# language: fr
Fonctionnalité: Redirection automatique si déjà connecté
  En tant qu'utilisateur déjà connecté je veux arriver directement sur mon tableau
  de bord quand j'ouvre l'application, plutôt que de revoir le formulaire de connexion

  Scénario: Session valide
    Etant donné que ma session est valide
    Quand j'arrive sur la page de connexion
    Alors je suis redirigé vers le tableau de bord

  Scénario: Pas de session
    Etant donné que je n'ai pas de session
    Quand j'arrive sur la page de connexion
    Alors je vois le formulaire de connexion
