# language: fr
Fonctionnalité: Demande de code de connexion (E2E nominal)
  Parcours contre la vraie API et une vraie base de données, sans mock.

  Scénario: Un utilisateur demande un code de connexion
    Etant donné que je suis sur la page de connexion
    Quand je saisis un email valide et je valide le formulaire
    Alors je vois la page de confirmation d'envoi
