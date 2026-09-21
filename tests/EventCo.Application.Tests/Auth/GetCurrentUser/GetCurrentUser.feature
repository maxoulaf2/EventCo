# language: fr
Fonctionnalité: Consultation de l'utilisateur courant
  En tant qu'utilisateur connecté je veux récupérer mes informations de profil
  afin que le frontend puisse les afficher (nom, email)

  Scénario: Consultation par un utilisateur connecté
    Etant donné un compte connecté pour "alice@example.com"
    Quand je consulte mes informations de profil
    Alors la consultation de mon profil réussit
    Et mes informations de profil correspondent à "alice@example.com"
