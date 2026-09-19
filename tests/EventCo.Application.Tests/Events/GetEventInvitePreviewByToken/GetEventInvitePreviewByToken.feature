# language: fr
Fonctionnalité: Aperçu d'un événement via son lien d'invitation
  En tant que visiteur non connecté je veux voir un aperçu de l'événement avant de me connecter
  afin de savoir à quoi je vais adhérer en cliquant sur le lien

  Scénario: Aperçu avec un lien valide
    Etant donné un événement avec aperçu "Repas de Noël" prévu le "2026-12-24" au lieu "Chez Alice"
    Quand je consulte l'aperçu de cet événement via son lien d'invitation
    Alors la consultation de l'aperçu réussit
    Et l'aperçu indique le titre "Repas de Noël"
    Et l'aperçu indique le lieu "Chez Alice"

  Scénario: Aperçu avec un lien d'invitation inexistant
    Quand je consulte l'aperçu d'un événement via un lien d'invitation inexistant
    Alors la consultation de l'aperçu échoue avec une erreur de lien introuvable
