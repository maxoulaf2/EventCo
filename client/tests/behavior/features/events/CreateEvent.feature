# language: fr
Fonctionnalité: Création d'un événement
  En tant qu'utilisateur connecté je veux créer un événement afin de pouvoir
  ensuite y inviter des participants et y organiser des tâches

  Contexte:
    Etant donné que je suis sur la page de création d'événement

  Scénario: Titre et date valides
    Quand je saisis le titre "Repas de Noël" et la date "2026-12-24" puis je valide le formulaire
    Alors je suis redirigé vers le tableau de bord

  Scénario: Le serveur refuse la création
    Et que le serveur refusera la prochaine création d'événement
    Quand je saisis le titre "Repas de Noël" et la date "2026-12-24" puis je valide le formulaire
    Alors je vois un message d'erreur sur le formulaire
    Et je reste sur la page de création d'événement
