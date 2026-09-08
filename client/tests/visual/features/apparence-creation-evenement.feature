# language: fr
Fonctionnalité: Non-régression visuelle de la page de création d'événement

  Scénario: Formulaire de création vide
    Etant donné que je suis sur le formulaire de création d'événement
    Alors son apparence correspond à la référence enregistrée

  Scénario: Formulaire de création avec un message d'erreur
    Etant donné que je suis sur le formulaire de création d'événement avec un message d'erreur
    Alors son apparence correspond à la référence enregistrée
