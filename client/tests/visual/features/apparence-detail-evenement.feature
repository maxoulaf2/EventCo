# language: fr
Fonctionnalité: Non-régression visuelle du détail d'un événement

  Scénario: Détail d'un événement vu par son créateur
    Etant donné que je suis sur le détail d'un événement en tant que créateur
    Alors son apparence correspond à la référence enregistrée

  Scénario: Détail d'un événement vu par un simple participant
    Etant donné que je suis sur le détail d'un événement en tant que simple participant
    Alors son apparence correspond à la référence enregistrée

  Scénario: Détail d'un événement avec une erreur d'invitation
    Etant donné que je suis sur le détail d'un événement avec une erreur d'invitation
    Alors son apparence correspond à la référence enregistrée

  Scénario: Détail d'un événement avec l'onglet des tâches faites
    Etant donné que je suis sur le détail d'un événement avec l'onglet des tâches faites
    Alors son apparence correspond à la référence enregistrée

  Scénario: Détail d'un événement avec le formulaire d'ajout de tâche rempli
    Etant donné que je suis sur le détail d'un événement avec le formulaire d'ajout de tâche rempli
    Alors son apparence correspond à la référence enregistrée
