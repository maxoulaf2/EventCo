# language: fr
Fonctionnalité: Déconnexion
  En tant qu'utilisateur connecté je veux pouvoir me déconnecter depuis la modale
  de mon compte afin de fermer ma session sur cet appareil

  Scénario: Ouverture et fermeture de la modale de mon compte
    Quand j'arrive sur le tableau de bord
    Et j'ouvre la modale de mon compte
    Alors je vois le bouton de déconnexion
    Quand je ferme la modale de mon compte
    Alors je ne vois plus le bouton de déconnexion

  Scénario: Déconnexion depuis la modale de mon compte
    Quand j'arrive sur le tableau de bord
    Et j'ouvre la modale de mon compte
    Et je clique sur le bouton de déconnexion
    Alors je suis redirigé vers la page de connexion
