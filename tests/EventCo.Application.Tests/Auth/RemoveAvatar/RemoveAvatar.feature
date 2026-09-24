# language: fr
Fonctionnalité: Suppression de la photo de profil
  En tant qu'utilisateur connecté je veux pouvoir retirer ma photo de profil
  afin de revenir à l'avatar par défaut (initiale de mon nom)

  Scénario: Suppression d'une photo existante
    Etant donné un compte ayant une photo de profil pour "alice@example.com"
    Quand je supprime ma photo de profil
    Alors la suppression de la photo de profil réussit
    Et je n'ai plus de photo de profil
    Et le fichier de ma photo de profil a été supprimé du stockage

  Scénario: Suppression sans photo existante
    Etant donné un compte n'ayant jamais eu de photo de profil pour "alice@example.com"
    Quand je supprime ma photo de profil
    Alors la suppression de la photo de profil réussit
    Et je n'ai plus de photo de profil
